﻿using System;
using System.Linq;
using System.Threading;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="DeviceMonitor"/> class.
    /// </summary>
    public class DeviceMonitorTests : SocketBasedTests
    {
        // Toggle the integration test flag to true to run on an actual adb server
        // (and to build/validate the test cases), set to false to use the mocked
        // adb sockets.
        // In release mode, this flag is ignored and the mocked adb sockets are always used.
        public DeviceMonitorTests() : base(integrationTest: false, doDispose: true)
        {
        }

        [Fact]
        public void ConstructorTest()
        {
            using DeviceMonitor monitor = new(Socket);
            Assert.NotNull(monitor.Devices);
            Assert.Equal(0, monitor.Devices.Count);
            Assert.Equal(Socket, monitor.Socket);
            Assert.False(monitor.IsRunning);
        }

        [Fact]
        public void ConstructorNullTest() => _ = Assert.Throws<ArgumentNullException>(() => new DeviceMonitor(null));

        [Fact]
        public void DeviceDisconnectedTest()
        {
            Socket.WaitForNewData = true;

            using DeviceMonitor monitor = new(Socket);
            DeviceMonitorSink sink = new(monitor);

            Assert.Equal(0, monitor.Devices.Count);

            // Start the monitor, detect the initial device.
            RunTest(
            OkResponse,
            ResponseMessages("169.254.109.177:5555\tdevice\n"),
            Requests("host:track-devices"),
            () =>
            {
                monitor.Start();

                Assert.Equal(1, monitor.Devices.Count);
                Assert.Single(sink.ConnectedEvents);
                Assert.Empty(sink.ChangedEvents);
                Assert.Empty(sink.DisconnectedEvents);
            });

            Socket.ResponseMessages.Clear();
            Socket.Responses.Clear();
            Socket.Requests.Clear();

            // Device disconnects
            ManualResetEvent eventWaiter = sink.CreateEventSignal();

            RunTest(
            NoResponses,
            ResponseMessages(""),
            Requests(),
            () =>
            {
                eventWaiter.WaitOne(1000);
                Assert.Equal(0, monitor.Devices.Count);
                Assert.Single(sink.ConnectedEvents);
                Assert.Empty(sink.ChangedEvents);
                Assert.Single(sink.DisconnectedEvents);
                Assert.Equal("169.254.109.177:5555", sink.DisconnectedEvents[0].Device.Serial);
            });
        }

        [Fact]
        public void DeviceConnectedTest()
        {
            Socket.WaitForNewData = true;

            using DeviceMonitor monitor = new(Socket);
            DeviceMonitorSink sink = new(monitor);

            Assert.Equal(0, monitor.Devices.Count);

            // Start the monitor, detect the initial device.
            RunTest(
            OkResponse,
            ResponseMessages(""),
            Requests("host:track-devices"),
            () =>
            {
                monitor.Start();

                Assert.Equal(0, monitor.Devices.Count);
                Assert.Empty(sink.ConnectedEvents);
                Assert.Empty(sink.ChangedEvents);
                Assert.Empty(sink.DisconnectedEvents);
            });

            Socket.ResponseMessages.Clear();
            Socket.Responses.Clear();
            Socket.Requests.Clear();

            // Device disconnects
            ManualResetEvent eventWaiter = sink.CreateEventSignal();

            RunTest(
            NoResponses,
            ResponseMessages("169.254.109.177:5555\tdevice\n"),
            Requests(),
            () =>
            {
                eventWaiter.WaitOne(1000);

                Assert.Equal(1, monitor.Devices.Count);
                Assert.Single(sink.ConnectedEvents);
                Assert.Empty(sink.ChangedEvents);
                Assert.Empty(sink.DisconnectedEvents);
                Assert.Equal("169.254.109.177:5555", sink.ConnectedEvents[0].Device.Serial);
            });
        }

        [Fact]
        public void StartInitialDeviceListTest()
        {
            lock (FactoriesTests.locker)
            {
                Socket.WaitForNewData = true;

                using DeviceMonitor monitor = new(Socket);
                DeviceMonitorSink sink = new(monitor);

                Assert.Equal(0, monitor.Devices.Count);

                RunTest(
                OkResponse,
                ResponseMessages("169.254.109.177:5555\tdevice\n"),
                Requests("host:track-devices"),
                () =>
                {
                    monitor.Start();

                    Assert.Equal(1, monitor.Devices.Count);
                    Assert.Equal("169.254.109.177:5555", monitor.Devices.ElementAt(0).Serial);
                    Assert.Single(sink.ConnectedEvents);
                    Assert.Equal("169.254.109.177:5555", sink.ConnectedEvents[0].Device.Serial);
                    Assert.Empty(sink.ChangedEvents);
                    Assert.Empty(sink.DisconnectedEvents);
                });
            }
        }

        [Fact]
        public void DeviceChangedTest()
        {
            Socket.WaitForNewData = true;

            using DeviceMonitor monitor = new(Socket);
            DeviceMonitorSink sink = new(monitor);

            Assert.Equal(0, monitor.Devices.Count);

            // Start the monitor, detect the initial device.
            RunTest(
            OkResponse,
            ResponseMessages("169.254.109.177:5555\toffline\n"),
            Requests("host:track-devices"),
            () =>
            {
                monitor.Start();

                Assert.Equal(1, monitor.Devices.Count);
                Assert.Equal(DeviceState.Offline, monitor.Devices.ElementAt(0).State);
                Assert.Single(sink.ConnectedEvents);
                Assert.Empty(sink.ChangedEvents);
                Assert.Empty(sink.DisconnectedEvents);
            });

            Socket.ResponseMessages.Clear();
            Socket.Responses.Clear();
            Socket.Requests.Clear();

            // Device disconnects
            ManualResetEvent eventWaiter = sink.CreateEventSignal();

            RunTest(
            NoResponses,
            ResponseMessages("169.254.109.177:5555\tdevice\n"),
            Requests(),
            () =>
            {
                eventWaiter.WaitOne(1000);

                Assert.Equal(1, monitor.Devices.Count);
                Assert.Equal(DeviceState.Online, monitor.Devices.ElementAt(0).State);
                Assert.Single(sink.ConnectedEvents);
                Assert.Single(sink.ChangedEvents);
                Assert.Empty(sink.DisconnectedEvents);
                Assert.Equal("169.254.109.177:5555", sink.ChangedEvents[0].Device.Serial);
            });
        }

        /// <summary>
        /// Tests the <see cref="DeviceMonitor"/> in a case where the adb server dies in the middle of the monitor
        /// loop. The <see cref="DeviceMonitor"/> should detect this condition and restart the adb server.
        /// </summary>
        [Fact]
        public void AdbKilledTest()
        {
            DummyAdbServer dummyAdbServer = new();
            AdbServer.Instance = dummyAdbServer;

            Socket.WaitForNewData = true;

            using DeviceMonitor monitor = new(Socket);
            RunTest(
            new AdbResponse[] { AdbResponse.OK, AdbResponse.OK },
            ResponseMessages(
                DummyAdbSocket.ServerDisconnected,
                string.Empty),
            Requests(
                "host:track-devices",
                "host:track-devices"),
            () =>
            {
                monitor.Start();

                Assert.True(Socket.DidReconnect);
                Assert.True(dummyAdbServer.WasRestarted);
            });
        }
    }
}

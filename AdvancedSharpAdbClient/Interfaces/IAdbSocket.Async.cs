﻿using System.Threading;
using System.Threading.Tasks;

namespace AdvancedSharpAdbClient
{
    public partial interface IAdbSocket
    {
        /// <summary>
        /// Sends the specified number of bytes of data to a <see cref="IAdbSocket"/>,
        /// </summary>
        /// <param name="data">A <see cref="byte"/> array that acts as a buffer, containing the data to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <param name="length">The number of bytes to send.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task SendAsync(byte[] data, int length, CancellationToken cancellationToken);

        /// <summary>
        /// Sends the specified number of bytes of data to a <see cref="IAdbSocket"/>,
        /// </summary>
        /// <param name="data">A <see cref="byte"/> array that acts as a buffer, containing the data to send.</param>
        /// <param name="offset">The index of the first byte in the array to send.</param>
        /// <param name="length">The number of bytes to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task SendAsync(byte[] data, int offset, int length, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously sends a request to the Android Debug Bridge.To read the response, call
        /// <see cref="ReadAdbResponseAsync(CancellationToken)"/>.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task SendAdbRequestAsync(string request, CancellationToken cancellationToken);

        /// <summary>
        /// Reads a <see cref="string"/> from an <see cref="IAdbSocket"/> instance when
        /// the connection is in sync mode.
        /// </summary>
        /// <param name="data" >The buffer to store the read data into.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task ReadAsync(byte[] data, CancellationToken cancellationToken);

        /// <summary>
        /// Receives data from a <see cref="IAdbSocket"/> into a receive buffer.
        /// </summary>
        /// <param name="data">An array of type <see cref="byte"/> that is the storage location for the received data.</param>
        /// <param name="length">The number of bytes to receive.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <remarks>Cancelling the task will also close the socket.</remarks>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The result value of the task contains the number of bytes received.</returns>
        Task<int> ReadAsync(byte[] data, int length, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously reads a <see cref="string"/> from an <see cref="IAdbSocket"/> instance.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The return value of the task is the<see cref="string"/> received from the <see cref = "IAdbSocket"/>.</returns>
        Task<string> ReadStringAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously receives an <see cref="AdbResponse"/> message, and throws an error
        /// if the message does not indicate success.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="AdbResponse"/> object that represents the response from the Android Debug Bridge.</returns>
        Task<AdbResponse> ReadAdbResponseAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Ask to switch the connection to the device/emulator identified by
        /// <paramref name="device"/>. After this request, every client request will
        /// be sent directly to the adbd daemon running on the device.
        /// </summary>
        /// <param name="device">The device to which to connect.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>If <paramref name="device"/> is <see langword="null"/>, this method does nothing.</remarks>
        Task SetDeviceAsync(DeviceData device, CancellationToken cancellationToken);
    }
}

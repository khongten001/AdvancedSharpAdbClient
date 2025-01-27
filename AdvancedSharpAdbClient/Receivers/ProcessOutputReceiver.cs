﻿// <copyright file="ProcessOutputReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.DeviceCommands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AdvancedSharpAdbClient.Receivers
{
    /// <summary>
    /// Parses the output of a <c>cat /proc/[pid]/stat</c> command.
    /// </summary>
    internal class ProcessOutputReceiver : MultiLineReceiver
    {
        /// <summary>
        /// Gets a list of all processes that have been received.
        /// </summary>
        public Collection<AndroidProcess> Processes { get; private set; } = new Collection<AndroidProcess>();

        /// <inheritdoc/>
        protected override void ProcessNewLines(IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                // Process has already died (e.g. the cat process itself)
                if (line.Contains("No such file or directory"))
                {
                    continue;
                }

                try
                {
                    Processes.Add(AndroidProcess.Parse(line, cmdLinePrefix: true));
                }
                catch (Exception)
                {
                    // Swallow
                }
            }
        }
    }
}

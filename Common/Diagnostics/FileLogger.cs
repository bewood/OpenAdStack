﻿//-----------------------------------------------------------------------
// <copyright file="FileLogger.cs" company="Rare Crowds Inc">
// Copyright 2012-2013 Rare Crowds, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Diagnostics
{
    /// <summary>
    /// Outputs log messages as human readable messages to a file.
    /// </summary>
    public class FileLogger : ILogger
    {
        /// <summary>The log file</summary>
        private readonly FileInfo logFile;

        /// <summary>Initializes a new instance of the FileLogger class</summary>
        /// <param name="fileName">Name of the file to write log output to</param>
        public FileLogger(string fileName)
        {
            this.logFile = new FileInfo(fileName);
        }

        /// <summary>Gets the log levels supported by this logger</summary>
        public LogLevels LogLevels
        {
            get { return LogLevels.All; }
        }

        /// <summary>Gets a value indicating whether only alerts are supported</summary>
        public bool AlertsOnly
        {
            get { return false; }
        }

        /// <summary>Logs a message with the specified log level</summary>
        /// <param name="level">The level of the log message</param>
        /// <param name="instance">The role instance of the log message</param>
        /// <param name="thread">The thread of the log message</param>
        /// <param name="source">The source of the log message</param>
        /// <param name="message">The content of the log message</param>
        public void LogMessage(LogLevels level, string instance, string thread, string source, string message)
        {
            var logMessage = string.Format(
                CultureInfo.InvariantCulture,
                "<<{0}>> [{1}] ({2}:{3}) {4} - {5}",
                Enum.GetName(typeof(LogLevels), level),
                DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                instance,
                thread,
                source,
                message);
            using (var writer = this.logFile.AppendText())
            {
                writer.WriteLine(logMessage);
            }
        }
    }
}

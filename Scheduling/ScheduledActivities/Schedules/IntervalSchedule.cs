﻿//-----------------------------------------------------------------------
// <copyright file="IntervalSchedule.cs" company="Rare Crowds Inc">
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

namespace ScheduledActivities.Schedules
{
    /// <summary>Schedule that runs on a TimeSpan interval</summary>
    /// <seealso cref="System.TimeSpan"/>
    public class IntervalSchedule : IActivitySourceSchedule
    {
        /// <summary>Initializes a new instance of the IntervalSchedule class.</summary>
        /// <param name="hours">Hours between schedule times</param>
        /// <param name="minutes">Minutes between schedule times</param>
        /// <param name="seconds">Seconds between schedule times</param>
        public IntervalSchedule(int hours, int minutes, int seconds)
            : this(new TimeSpan(hours, minutes, seconds))
        {
        }

        /// <summary>Initializes a new instance of the IntervalSchedule class.</summary>
        /// <param name="days">Days between schedule times</param>
        /// <param name="hours">Hours between schedule times</param>
        /// <param name="minutes">Minutes between schedule times</param>
        /// <param name="seconds">Seconds between schedule times</param>
        public IntervalSchedule(int days, int hours, int minutes, int seconds)
            : this(new TimeSpan(days, hours, minutes, seconds))
        {
        }

        /// <summary>Initializes a new instance of the IntervalSchedule class.</summary>
        /// <param name="days">Days between schedule times</param>
        /// <param name="hours">Hours between schedule times</param>
        /// <param name="minutes">Minutes between schedule times</param>
        /// <param name="seconds">Seconds between schedule times</param>
        /// <param name="milliseconds">Milliseconds between schedule times</param>
        public IntervalSchedule(int days, int hours, int minutes, int seconds, int milliseconds)
            : this(new TimeSpan(days, hours, minutes, seconds, milliseconds))
        {
        }

        /// <summary>Initializes a new instance of the IntervalSchedule class.</summary>
        /// <remarks>
        /// Parses the interval using <see cref="System.TimeSpan.Parse(string)"/>.
        /// See http://msdn.microsoft.com/en-us/library/se73z7b9.aspx for format information.
        /// </remarks>
        /// <param name="interval">String containing an interval</param>
        /// <seealso cref="System.TimeSpan.Parse(string)"/>
        public IntervalSchedule(string interval)
            : this(TimeSpan.Parse(interval, CultureInfo.InvariantCulture))
        {
        }

        /// <summary>Initializes a new instance of the IntervalSchedule class.</summary>
        /// <param name="interval">Interval between schedule times</param>
        private IntervalSchedule(TimeSpan interval)
        {
            this.Interval = interval;
        }

        /// <summary>Gets the interval for the schedule</summary>
        public TimeSpan Interval { get; private set; }

        /// <summary>Gets the next schedule time</summary>
        /// <param name="lastTime">The last time the source was checked</param>
        /// <returns>The next time the source should be checked</returns>
        public DateTime GetNextTime(DateTime lastTime)
        {
            return lastTime + this.Interval;
        }
    }
}

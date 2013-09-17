﻿//-----------------------------------------------------------------------
// <copyright file="TimeOfDaySchedule.cs" company="Rare Crowds Inc">
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
    /// <summary>Schedule that runs at a specific time of day</summary>
    /// <seealso cref="System.DateTime.TimeOfDay"/>
    /// <seealso cref="System.TimeSpan"/>
    public class TimeOfDaySchedule : IActivitySourceSchedule
    {
        /// <summary>Initializes a new instance of the TimeOfDaySchedule class.</summary>
        /// <param name="hour">Hour of the day (UTC)</param>
        /// <param name="minute">Minute of the day (UTC)</param>
        /// <param name="second">Second of the day (UTC)</param>
        public TimeOfDaySchedule(int hour, int minute, int second)
            : this(new TimeSpan(hour, minute, second))
        {
        }

        /// <summary>Initializes a new instance of the TimeOfDaySchedule class.</summary>
        /// <remarks>
        /// Parses the time of day using <see cref="System.TimeSpan.Parse(string)"/>.
        /// See http://msdn.microsoft.com/en-us/library/se73z7b9.aspx for format information.
        /// </remarks>
        /// <param name="timeOfDay">String containing the time of day</param>
        /// <seealso cref="System.TimeSpan.Parse(string)"/>
        public TimeOfDaySchedule(string timeOfDay)
            : this(TimeSpan.Parse(timeOfDay, CultureInfo.InvariantCulture))
        {
        }

        /// <summary>Initializes a new instance of the TimeOfDaySchedule class.</summary>
        /// <param name="timeOfDay">Time of day of the schedule</param>
        private TimeOfDaySchedule(TimeSpan timeOfDay)
        {
            this.TimeOfDay = timeOfDay;
        }

        /// <summary>Gets the interval for the schedule</summary>
        public TimeSpan TimeOfDay { get; private set; }

        /// <summary>Gets the DateTime representing today</summary>
        private static DateTime Today
        {
            get { return DateTime.UtcNow.Date; }
        }

        /// <summary>Gets the DateTime representing tomorrow</summary>
        private static DateTime Tomorrow
        {
            get { return Today + new TimeSpan(1, 0, 0, 0); }
        }

        /// <summary>Gets the next schedule time</summary>
        /// <param name="lastTime">The last time the source was checked</param>
        /// <returns>The next time the source should be checked</returns>
        public DateTime GetNextTime(DateTime lastTime)
        {
            // If not yet run today, return the time of day for today.
            // Otherwise, return the time of day for tomorrow.
            return lastTime < Today + this.TimeOfDay ?
                Today + this.TimeOfDay :
                Tomorrow + this.TimeOfDay;
        }
    }
}

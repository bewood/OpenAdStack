// -----------------------------------------------------------------------
// <copyright file="IDeliveryDataCost.cs" company="Rare Crowds Inc">
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
// -----------------------------------------------------------------------

using DynamicAllocation;

namespace DynamicAllocationActivities
{
    /// <summary>Data coster interface.</summary>
    public interface IDeliveryDataCost
    {
        /// <summary>Calculate the costs associated with data delivery for a single hour.</summary>
        /// <param name="impressions">Impression count.</param>
        /// <param name="mediaSpend">Media spend.</param>
        /// <param name="measureSet">Measure set of node.</param>
        /// <returns>Delivery costs.</returns>
        decimal CalculateHourCost(long impressions, decimal mediaSpend, MeasureSet measureSet);
    }
}
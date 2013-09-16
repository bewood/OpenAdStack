﻿// -----------------------------------------------------------------------
// <copyright file="AllocateExportSlotsQuickly.cs" company="Rare Crowds Inc">
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SellSideAllocation
{
    /// <summary>
    /// Class for quickly allocating export slots 
    /// </summary>
    internal static class AllocateExportSlotsQuickly
    {
        /// <summary>
        /// Will distribute export slots among the layers to maximize the total value using a greedy approximation
        /// </summary>
        /// <param name="node">a node whose children have the AverageValue populated and are sorted by AverageValue descending</param>
        /// <returns>the list of Layers with the ExportSlots populated to maximize the total value</returns>
        public static Node AllocateSlots(Node node)
        {
            var bestPartition = new int[node.ChildNodes.Count()];

            for (var i = 0; i < node.ExportSlots; i++)
            {
                bestPartition = AddSlotToPartition(bestPartition, node);
            }

            for (var i = 0; i < bestPartition.Length; i++)
            {
                node.ChildNodes[i].ExportSlots = bestPartition[i];
            }

            node = AllocateExportSlots.VolumeBudgetLayers(node);

            return node;
        }

        /// <summary>
        /// Add an export slot to the partition in the best way
        /// </summary>
        /// <param name="partition">the current partition</param>
        /// <param name="node">the parent node</param>
        /// <returns>the partition with the new slot added</returns>
        internal static int[] AddSlotToPartition(int[] partition, Node node)
        {
            int bestLayerIndexToAddTo = 0;
            var bestLayerScore = decimal.MinValue;

            // temporarily set the node's export slots for the sake of PartitionIsFeasible
            var originalExportSlots = node.ExportSlots;
            node.ExportSlots = partition.Sum() + 1;

            for (var i = 0; i < node.ChildNodes.Count(); i++)
            {
                partition[i]++;
                var layerScore = decimal.MinValue;
                if (AllocateExportSlots.PartitionIsFeasible(partition, node))
                {
                    layerScore = AllocateExportSlots.LayerAllocationScore(partition, node);
                }

                partition[i]--;

                if (layerScore > bestLayerScore)
                {
                    bestLayerIndexToAddTo = i;
                    bestLayerScore = layerScore;
                }
            }

            // restore the original value
            node.ExportSlots = originalExportSlots;

            partition[bestLayerIndexToAddTo]++;
            return partition;
        }
    }
}

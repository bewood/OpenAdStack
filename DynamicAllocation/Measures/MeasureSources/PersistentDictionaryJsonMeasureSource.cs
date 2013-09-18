//-----------------------------------------------------------------------
// <copyright file="PersistentDictionaryJsonMeasureSource.cs" company="Rare Crowds Inc">
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
using System.IO;
using System.Reflection;
using Utilities.Storage;

namespace DynamicAllocation
{
    /// <summary>Source for measures loaded from persistent dictionary JSON resources</summary>
    public class PersistentDictionaryJsonMeasureSource : JsonMeasureSource, IMeasureSource
    {
        /// <summary>Backing field for JsonMeasureSource.MeasureJson</summary>
        private string measureJson;

        /// <summary>Persistent dictionary store name</summary>
        private string storeName;

        /// <summary>Persistent dictionary key</summary>
        private string dictionaryKey;

        /// <summary>Initializes a new instance of the PersistentDictionaryJsonMeasureSource class.</summary>
        /// <param name="storeName">Dictionary store name</param>
        /// <param name="dictionaryKey">Dictionary key</param>
        public PersistentDictionaryJsonMeasureSource(string storeName, string dictionaryKey)
            : base("RESOURCE:{0}-{1}".FormatInvariant(storeName, dictionaryKey))
        {
            this.storeName = storeName;
            this.dictionaryKey = dictionaryKey;
        }

        /// <summary>
        /// Gets the JSON containing this source's measures loaded from an embedded resource.
        /// </summary>
        protected override string MeasureJson
        {
            get
            {
                return this.measureJson =
                    this.measureJson ??
                    this.LoadMeasuresFromPersistentDictionary();
            }
        }

        /// <summary>Loads measures JSON from a persistent dictionary</summary>
        /// <returns>Loaded JSON</returns>
        private string LoadMeasuresFromPersistentDictionary()
        {
            var dictionary = PersistentDictionaryFactory.CreateDictionary<string>(this.storeName);
            var json = dictionary[this.dictionaryKey];
            return json;
        }
    }
}

﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeyRuleFactoryFixture.cs" company="Rare Crowds Inc">
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
// --------------------------------------------------------------------------------------------------------------------

using ConcreteDataStore;
using DataAccessLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConcreteDataStoreUnitTests
{
    /// <summary>Test fixture for KeyRuleFactory.</summary>
    [TestClass]
    public class KeyRuleFactoryFixture
    {
        /// <summary>Default construction should give us a valid IKeyRuleFactory</summary>
        [TestMethod]
        public void DefaultConstruction()
        {
            var keyRule = new KeyRuleFactory() as IKeyRuleFactory;
            Assert.IsNotNull(keyRule);
        }

        /// <summary>Test we can retrieve a key rule</summary>
        [TestMethod]
        public void GetKeyRule()
        {
            var keyRule = new KeyRuleFactory();
            keyRule.GetKeyRule(new Entity(), "Azure", "Partition");
        }
    }
}

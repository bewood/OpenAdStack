﻿//-----------------------------------------------------------------------
// <copyright file="ExtensionsFixture.cs" company="Rare Crowds Inc">
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

using System;
using System.Collections.Generic;
using DataAccessLayer;
using EntityUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityUtilitiesUnitTests
{
    /// <summary>Test for entity extensions</summary>
    [TestClass]
    public class ExtensionsFixture
    {
        /// <summary>List of company entities used for testing</summary>
        private List<CompanyEntity> entities;

        /// <summary>Creates the list of entities for the serialize/deserialize extensions</summary>
        [TestInitialize]
        public void InitializeTest()
        {
            var companyAJson =
                @"{ ""ExternalName"":""CompanyFoo"",
                    ""ExternalType"":""Company.Agency""
                  }";
            var companyBJson =
                @"{ ""ExternalName"":""CompanyBar"",
                    ""ExternalType"":""Company.Advertiser""
                  }";

            this.entities = new List<CompanyEntity>
            {
                EntityJsonSerializer.DeserializeCompanyEntity(new EntityId(), companyAJson),
                EntityJsonSerializer.DeserializeCompanyEntity(new EntityId(), companyBJson)
            };
        }
        
        /// <summary>Verify serializing a list of entities to json and back</summary>
        [TestMethod]
        public void JsonEntityListSerialization()
        {
            var json = this.entities.SerializeToJson();

            Assert.IsTrue(json.StartsWith("["));
            Assert.IsTrue(json.EndsWith("]"));
            foreach (var entity in this.entities)
            {
                Assert.IsTrue(json.Contains(((EntityId)entity.ExternalEntityId).ToString()));
                Assert.IsTrue(json.Contains(entity.ExternalName));
                Assert.IsTrue(json.Contains(entity.ExternalType));
            }
        }

        /// <summary>Test we can serialize the entity to a json object.</summary>
        [TestMethod]
        public void SerializeCompanyToJson()
        {
            var companyEntity = new CompanyEntity(new Entity
            {
                ExternalEntityId = new EntityProperty("ExternalEntityId", new EntityId()),
                ExternalName = new EntityProperty("ExternalName", "Test Company"),
                EntityCategory = new EntityProperty("EntityCategory", CompanyEntity.CategoryName),
                ExternalType = new EntityProperty("ExternalType", "Agency"),
                CreateDate = new EntityProperty("CreateDate", DateTime.Now),
                LastModifiedDate = new EntityProperty("LastModifiedDate", DateTime.Now),
                LocalVersion = new EntityProperty("LocalVersion", 1)
            });
            string jsonEntity = companyEntity.SerializeToJson();

            // Round-trip the entity
            var newCompanyEntity = EntityJsonSerializer.DeserializeCompanyEntity(companyEntity.ExternalEntityId, jsonEntity);

            // Assert that the new Entity has the same Company elements as the derserialized entity
            // We currently don't use any properties beyond IEntity but assert those that are relevant
            Assert.AreEqual(companyEntity.Properties.Count, newCompanyEntity.Properties.Count);
            Assert.AreEqual(companyEntity.EntityCategory, newCompanyEntity.EntityCategory);
            Assert.AreEqual(companyEntity.ExternalName, newCompanyEntity.ExternalName);
            Assert.AreEqual(companyEntity.ExternalType, newCompanyEntity.ExternalType);
        }

        /// <summary>Test we can serialize the campaign entity to a json object.</summary>
        [TestMethod]
        public void SerializeCampaignToJson()
        {
            var campaignEntity = new CampaignEntity(new Entity
            {
                ExternalEntityId = new EntityId(),
                ExternalName = "Test Campaign",
                EntityCategory = CampaignEntity.CategoryName,
                ExternalType = "Campaign Type",
                CreateDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                LocalVersion = 1,
            });
            campaignEntity.Budget = 12345;

            string jsonEntity = campaignEntity.SerializeToJson();

            // Round-trip the entity
            var newCampaignEntity = EntityJsonSerializer.DeserializeCampaignEntity(campaignEntity.ExternalEntityId, jsonEntity);

            // Assert that the new Entity has the same Campaign elements as the derserialized entity
            Assert.AreEqual(campaignEntity.Properties.Count, newCampaignEntity.Properties.Count);
            Assert.AreEqual(campaignEntity.EntityCategory, newCampaignEntity.EntityCategory);
            Assert.AreEqual(campaignEntity.ExternalName, newCampaignEntity.ExternalName);
            Assert.AreEqual((double)campaignEntity.Budget, (double)newCampaignEntity.Budget);
            Assert.AreEqual(campaignEntity.StartDate, newCampaignEntity.StartDate);
            Assert.AreEqual(campaignEntity.EndDate, newCampaignEntity.EndDate);
            Assert.AreEqual(campaignEntity.PersonaName, newCampaignEntity.PersonaName);
        }

        /// <summary>Test we can deserialize the campaign entity from json</summary>
        [TestMethod]
        public void DeserializeCampaignFromJson()
        {
            const string CampaignJson =
@"{
  ""ExternalName"":""New Campaign"",
  ""Properties"":{
    ""Budget"":50000,
    ""CPM"":2.34,
    ""StartDate"":""2013-11-03T00:00:00.000Z"",
    ""EndDate"":""2013-11-24T10:00:00.000Z"",
    ""Status"":""Draft"",
    ""InventoryStrategy"":""2""
  },
  ""ExternalType"":""DynamicAllocationCampaign""
}";

            var deserialized = EntityJsonSerializer.DeserializeCampaignEntity(new EntityId(), CampaignJson);

            // Assert the deserialized campaign has the expected values
            Assert.AreEqual(6, deserialized.Properties.Count);
            Assert.AreEqual<string>("Campaign", deserialized.EntityCategory);
            Assert.AreEqual<string>("New Campaign", deserialized.ExternalName);
            Assert.AreEqual<double>(50000.00, deserialized.Budget);
            Assert.AreEqual<DateTime>(new DateTime(2013, 11, 03, 0, 0, 0, 0, DateTimeKind.Utc), deserialized.StartDate);
            Assert.AreEqual<DateTime>(new DateTime(2013, 11, 24, 10, 0, 0, 0, DateTimeKind.Utc), deserialized.EndDate);
            Assert.AreEqual<string>("DynamicAllocationCampaign", deserialized.ExternalType);
        }

        /// <summary>Test we can serialize the creative entity to a json object.</summary>
        [TestMethod]
        public void SerializeCreativeToJson()
        {
            var creativeEntity = new CreativeEntity(new Entity
            {
                ExternalEntityId = new EntityProperty("ExternalEntityId", new EntityId()),
                ExternalName = new EntityProperty("ExternalName", "Test Creative"),
                EntityCategory = new EntityProperty("EntityCategory", CreativeEntity.CategoryName),
                ExternalType = new EntityProperty("ExternalType", "FooThingy"),
                CreateDate = new EntityProperty("CreateDate", DateTime.Now),
                LastModifiedDate = new EntityProperty("LastModifiedDate", DateTime.Now),
                LocalVersion = new EntityProperty("LocalVersion", 1),
            });
            string jsonEntity = creativeEntity.SerializeToJson();

            // Round-trip the entity
            var newCreativeEntity = EntityJsonSerializer.DeserializeCreativeEntity(creativeEntity.ExternalEntityId, jsonEntity);

            // Assert that the new Entity has the same Creative elements as the derserialized entity
            Assert.AreEqual(creativeEntity.Properties.Count, newCreativeEntity.Properties.Count);
            Assert.AreEqual(creativeEntity.EntityCategory, newCreativeEntity.EntityCategory);
            Assert.AreEqual(creativeEntity.ExternalName, newCreativeEntity.ExternalName);
        }

        /// <summary>Test we can serialize the partner entity to a json object.</summary>
        [TestMethod]
        public void SerializePartnerToJson()
        {
            var partnerEntity = new PartnerEntity(new Entity
            {
                ExternalEntityId = new EntityProperty("ExternalEntityId", new EntityId()),
                ExternalName = new EntityProperty("ExternalName", "Test Partner Foo"),
                EntityCategory = new EntityProperty("EntityCategory", PartnerEntity.CategoryName),
                ExternalType = new EntityProperty("ExternalType", "Foo Type")
            });
            string jsonEntity = partnerEntity.SerializeToJson();

            // Round-trip the entity
            var newPartnerEntity = EntityJsonSerializer.DeserializePartnerEntity(partnerEntity.ExternalEntityId, jsonEntity);

            // Assert that the new Entity has the same elements as the derserialized entity
            Assert.AreEqual(partnerEntity.Properties.Count, newPartnerEntity.Properties.Count);
            Assert.AreEqual(partnerEntity.EntityCategory, newPartnerEntity.EntityCategory);
            Assert.AreEqual(partnerEntity.ExternalName, newPartnerEntity.ExternalName);
            Assert.AreEqual(partnerEntity.ExternalType, newPartnerEntity.ExternalType);
        }

        /// <summary>Test we can serialize the user entity to a json object.</summary>
        [TestMethod]
        public void SerializeUserToJson()
        {
            var userEntity = new UserEntity(new Entity
            {
                ExternalEntityId = new EntityProperty("ExternalEntityId", new EntityId()),
                ExternalName = new EntityProperty("ExternalName", "Test User"),
                EntityCategory = new EntityProperty("EntityCategory", UserEntity.CategoryName),
                ExternalType = new EntityProperty("ExternalType", "User Type"),
                CreateDate = new EntityProperty("CreateDate", DateTime.Now),
                LastModifiedDate = new EntityProperty("LastModifiedDate", DateTime.Now),
                LocalVersion = new EntityProperty("LocalVersion", 1),
            });
            string jsonEntity = userEntity.SerializeToJson();

            // Round-trip the entity
            var newUserEntity = EntityJsonSerializer.DeserializeUserEntity(userEntity.ExternalEntityId, jsonEntity);

            // Assert that the new Entity has the same User elements as the derserialized entity
            Assert.AreEqual(userEntity.Properties.Count, newUserEntity.Properties.Count);
            Assert.AreEqual(userEntity.EntityCategory, newUserEntity.EntityCategory);
            Assert.AreEqual(userEntity.UserId, newUserEntity.UserId);
            Assert.AreEqual(userEntity.FullName, newUserEntity.FullName);
            Assert.AreEqual(userEntity.FirstName, newUserEntity.FirstName);
            Assert.AreEqual(userEntity.LastName, newUserEntity.LastName);
            Assert.AreEqual(userEntity.ContactEmail, newUserEntity.ContactEmail);
            Assert.AreEqual(userEntity.ContactPhone, newUserEntity.ContactPhone);
        }

        /// <summary>Test that we can serialize and deserialize an association to JSON fragment for a collection.</summary>
        [TestMethod]
        public void RoundtripSerializeToJsonCollectionFragment()
        {
            var association = new Association
            {
                ExternalName = "MyFoos",
                AssociationType = AssociationType.Relationship,
                TargetEntityId = 1,
                TargetEntityCategory = CompanyEntity.CategoryName,
                TargetExternalType = "targetfoo"
            };
            var jsonDictionary = association.SerializeToJsonCollectionFragmentDictionary();

            // The dictionary should not contain the ExternalName property if this is targeted to a collection of associations.
            Assert.IsFalse(jsonDictionary.ContainsKey("ExternalName"));
            Assert.AreEqual(Association.StringFromAssociationType(association.AssociationType), jsonDictionary["AssociationType"]);
            Assert.AreEqual((string)association.TargetEntityId, jsonDictionary["TargetEntityId"]);
            Assert.AreEqual(association.TargetEntityCategory, jsonDictionary["TargetEntityCategory"]);
            Assert.AreEqual(association.TargetExternalType, jsonDictionary["TargetExternalType"]);
        }

        /// <summary>
        /// Test roundtripping a system property with an integer value
        /// </summary>
        [TestMethod]
        public void RoundtripSystemPropertyInteger()
        {
            var r = new Random();

            int expectedValue = r.Next();
            var propertyName = Guid.NewGuid().ToString("N");

            var entity = new PartnerEntity(new Entity
            {
                ExternalEntityId = new EntityProperty("ExternalEntityId", new EntityId()),
                ExternalName = new EntityProperty("ExternalName", "Test Partner Foo"),
                EntityCategory = new EntityProperty("EntityCategory", PartnerEntity.CategoryName),
                ExternalType = new EntityProperty("ExternalType", "Foo Type")
            });

            entity.SetSystemProperty(propertyName, expectedValue);
            var value = entity.GetSystemProperty<int>(propertyName);
            Assert.AreEqual(expectedValue, value);

            expectedValue = r.Next();
            entity.SetSystemProperty(propertyName, expectedValue);
            value = entity.GetSystemProperty<int>(propertyName);
            Assert.AreEqual(expectedValue, value);
        }

        /// <summary>
        /// Test roundtripping a system property with a decimal value
        /// </summary>
        [TestMethod]
        public void RoundtripSystemPropertyDecimal()
        {
            var r = new Random();

            decimal expectedValue = (decimal)(r.Next() * r.NextDouble());
            var propertyName = Guid.NewGuid().ToString("N");

            var entity = new PartnerEntity(new Entity
            {
                ExternalEntityId = new EntityProperty("ExternalEntityId", new EntityId()),
                ExternalName = new EntityProperty("ExternalName", "Test Partner Foo"),
                EntityCategory = new EntityProperty("EntityCategory", PartnerEntity.CategoryName),
                ExternalType = new EntityProperty("ExternalType", "Foo Type")
            });

            entity.SetSystemProperty(propertyName, expectedValue);
            var value = entity.GetSystemProperty<decimal>(propertyName);
            Assert.AreEqual(expectedValue, value);

            expectedValue = (decimal)(r.Next() * r.NextDouble());
            entity.SetSystemProperty(propertyName, expectedValue);
            value = entity.GetSystemProperty<decimal>(propertyName);
            Assert.AreEqual(expectedValue, value);
        }

        /// <summary>
        /// Test roundtripping a system property with a string value
        /// </summary>
        [TestMethod]
        public void RoundtripSystemPropertyString()
        {
            string expectedValue = Guid.NewGuid().ToString("N").Left(10);
            var propertyName = Guid.NewGuid().ToString("N");

            var entity = new PartnerEntity(new Entity
            {
                ExternalEntityId = new EntityProperty("ExternalEntityId", new EntityId()),
                ExternalName = new EntityProperty("ExternalName", "Test Partner Foo"),
                EntityCategory = new EntityProperty("EntityCategory", PartnerEntity.CategoryName),
                ExternalType = new EntityProperty("ExternalType", "Foo Type")
            });

            entity.SetSystemProperty(propertyName, expectedValue);
            var value = entity.GetSystemProperty<string>(propertyName);
            Assert.AreEqual(expectedValue, value);

            expectedValue = Guid.NewGuid().ToString("N").Right(10);
            entity.SetSystemProperty(propertyName, expectedValue);
            value = entity.GetSystemProperty<string>(propertyName);
            Assert.AreEqual(expectedValue, value);
        }

        /// <summary>
        /// Test roundtripping UserEntity.ExternalType via UserType extensions
        /// </summary>
        [TestMethod]
        public void RoundtripUserType()
        {
            const UserType Expected = UserType.Default;
            var user = new UserEntity(new Entity
            {
                ExternalEntityId = new EntityProperty("ExternalEntityId", new EntityId()),
                ExternalName = new EntityProperty("ExternalName", "Test User"),
                EntityCategory = new EntityProperty("EntityCategory", UserEntity.CategoryName),
                ExternalType = new EntityProperty("ExternalType", string.Empty)
            });
            Assert.AreEqual(UserType.Unknown, user.GetUserType());

            user.SetUserType(Expected);
            Assert.AreEqual(Expected, user.GetUserType());
        }

        /// <summary>
        /// Test attempting to set user ExternalType to UserType.Unknown via UserType extension throws ArgumentException
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotSetUserTypeUnknown()
        {
            var user = new UserEntity(new Entity
            {
                ExternalEntityId = new EntityProperty("ExternalEntityId", new EntityId()),
                ExternalName = new EntityProperty("ExternalName", "Test User"),
                EntityCategory = new EntityProperty("EntityCategory", UserEntity.CategoryName),
                ExternalType = new EntityProperty("ExternalType", string.Empty)
            });
            Assert.AreEqual(UserType.Unknown, user.GetUserType());

            user.SetUserType(UserType.Unknown);
        }

        /// <summary>
        /// Test roundtripping IEntity.OwnerId
        /// </summary>
        [TestMethod]
        public void RoundtripOwnerId()
        {
            var ownerId = Guid.NewGuid().ToString("N");
            var entity = new PartnerEntity(new Entity
            {
                ExternalEntityId = new EntityProperty("ExternalEntityId", new EntityId()),
                ExternalName = new EntityProperty("ExternalName", "Test Partner Foo"),
                EntityCategory = new EntityProperty("EntityCategory", PartnerEntity.CategoryName),
                ExternalType = new EntityProperty("ExternalType", "Foo Type")
            });
            Assert.IsNull(entity.GetOwnerId());

            entity.SetOwnerId(ownerId);
            Assert.AreEqual(ownerId, entity.GetOwnerId());
        }

        /// <summary>
        /// Test defaulting to last modified user for entities without owner ids
        /// </summary>
        [TestMethod]
        public void DefaultOwnerIdIsNull()
        {
            var lastModifiedUserId = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
            var entity = new PartnerEntity(new Entity
            {
                ExternalEntityId = new EntityProperty("ExternalEntityId", new EntityId()),
                ExternalName = new EntityProperty("ExternalName", "Test Partner Foo"),
                EntityCategory = new EntityProperty("EntityCategory", PartnerEntity.CategoryName),
                ExternalType = new EntityProperty("ExternalType", "Foo Type"),
                LastModifiedUser = new EntityProperty("LastModifiedUser", lastModifiedUserId),
            });
            var defaultOwnerId = entity.GetOwnerId();
            Assert.IsNull(defaultOwnerId);
        }
    }
}
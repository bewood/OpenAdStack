﻿// -----------------------------------------------------------------------
// <copyright file="IPaymentProcessor.cs" company="Rare Crowds Inc">
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

namespace PaymentProcessor
{
    /// <summary>Interface to encapsulate payment processing implementations.</summary>
    public interface IPaymentProcessor
    {
        /// <summary>Gets the name of the PaymentProcessor.</summary>
        string ProcessorName { get; }

        /// <summary>Create a customer on the billing service.</summary>
        /// <param name="billingInformationToken">A token representing the customers billing information.</param>
        /// <param name="companyName">The internal name of the customer (company name).</param>
        /// <returns>A customer billing id.</returns>
        string CreateCustomer(string billingInformationToken, string companyName);

        /// <summary>Create a customer on the billing service.</summary>
        /// <param name="billingInformationToken">A token representing the customers billing information.</param>
        /// <param name="customerBillingId">The billing id of the customer.</param>
        /// <param name="companyName">The internal name of the customer (company name).</param>
        /// <returns>A customer billing id.</returns>
        string UpdateCustomer(string billingInformationToken, string customerBillingId, string companyName);

        /// <summary>Charge a customer on the billing service.</summary>
        /// <param name="customerBillingId">The billing id of the customer.</param>
        /// <param name="chargeAmount">Amount of charge in U.S. dollars.</param>
        /// <param name="chargeDescription">Description of charge.</param>
        /// <returns>A charge id.</returns>
        string ChargeCustomer(string customerBillingId, decimal chargeAmount, string chargeDescription);

        /// <summary>Refund a customer on the billing service.</summary>
        /// <param name="customerBillingId">The billing id of the customer.</param>
        /// <param name="chargeId">Id of charge to refund against.</param>
        /// <param name="refundAmount">Amount of refund in U.S. dollars.</param>
        /// <returns>A charge id.</returns>
        string RefundCustomer(string customerBillingId, string chargeId, decimal refundAmount);
        
        /// <summary>Perform a charge check (but not an actual charge) on the billing service.</summary>
        /// <param name="customerBillingId">The billing id of the customer.</param>
        /// <param name="chargeAmount">Amount of charge in U.S. dollars.</param>
        /// <param name="chargeDescription">Description of charge.</param>
        /// <returns>A charge id.</returns>
        string PerformChargeCheck(string customerBillingId, decimal chargeAmount, string chargeDescription);
    }
}

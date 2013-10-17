﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorResponse.cs" company="Rare Crowds Inc">
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
using System.Linq;
using System.Web;

namespace ApiLayer
{
    /// <summary>
    /// This class holds error data that is sent to client 
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Gets or sets external error id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets error message 
        /// </summary>
        public string Message { get; set; }              
    }
}
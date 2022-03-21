﻿/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Synapse.Integration.Commands.Correlations;
using Synapse.Integration.Commands.WorkflowInstances;
using Synapse.Integration.Commands.Workflows;
using Synapse.Integration.Models;
using System.ServiceModel;

namespace Synapse.Apis.Management
{

    /// <summary>
    /// Defines the fundamentals of the Synapse Management API 
    /// </summary>
    [ServiceContract]
    public interface ISynapseManagementApi
    {

        #region Workflows

        /// <summary>
        /// Creates a new <see cref="V1Workflow"/>
        /// </summary>
        /// <param name="command">The object that describes the command to execute</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The newly created <see cref="V1Workflow"/></returns>
        [OperationContract]
        Task<V1Workflow> CreateWorkflowAsync(V1CreateWorkflowCommand command, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the <see cref="V1Workflow"/> with the specified id
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Workflow"/> to get</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The <see cref="V1Workflow"/> with the specified id</returns>

        [OperationContract]
        Task<V1Workflow> GetWorkflowByIdAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists existing <see cref="V1Workflow"/>s
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="List{T}"/> containing all existing <see cref="V1Workflow"/>s</returns>
        [OperationContract]
        Task<List<V1Workflow>> GetWorkflowsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists existing <see cref="V1Workflow"/>s
        /// </summary>
        /// <param name="query">The OData query string</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="List{T}"/> containing all existing <see cref="V1Workflow"/>s</returns>
        [OperationContract]
        Task<List<V1Workflow>> GetWorkflowsAsync(string query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the <see cref="V1Workflow"/> with the specified id
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Workflow"/> to delete</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        [OperationContract]
        Task DeleteWorkflowAsync(string id, CancellationToken cancellationToken = default);

        #endregion

        #region WorkflowInstances

        /// <summary>
        /// Creates a new workflow instance
        /// </summary>
        /// <param name="command">The object that describes the command to execute</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The newly created workflow instance</returns>
        [OperationContract]
        Task<V1WorkflowInstance> CreateWorkflowInstanceAsync(V1CreateWorkflowInstanceCommand command, CancellationToken cancellationToken = default);

        /// <summary>
        /// Starts a workflow instance
        /// </summary>
        /// <param name="id">The id of the workflow instance to start</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The started workflow instance</returns>
        [OperationContract]
        Task<V1WorkflowInstance> StartWorkflowInstanceAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the workflow instance with the specified id
        /// </summary>
        /// <param name="id">The id of the workflow instance to get</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The workflow instance with the specified id</returns>
        [OperationContract]
        Task<V1WorkflowInstance> GetWorkflowInstanceByIdAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists existing workflow instances
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="List{T}"/> containing all existing workflow instances</returns>
        [OperationContract]
        Task<List<V1WorkflowInstance>> GetWorkflowInstancesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists existing workflow instances
        /// </summary>
        /// <param name="query">The ODATA query</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="List{T}"/> containing all existing workflow instances</returns>
        [OperationContract]
        Task<List<V1WorkflowInstance>> GetWorkflowInstancesAsync(string query, CancellationToken cancellationToken = default);

        #endregion

        #region Correlations

        /// <summary>
        /// Creates a new <see cref="V1Correlation"/>
        /// </summary>
        /// <param name="command">The object that describes the command to execute</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The newly created <see cref="V1Correlation"/></returns>
        [OperationContract]
        Task<V1Correlation> CreateCorrelationAsync(V1CreateCorrelationCommand command, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the <see cref="V1Correlation"/> with the specified id
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Correlation"/> to get</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The <see cref="V1Correlation"/> with the specified id</returns>

        [OperationContract]
        Task<V1Correlation> GetCorrelationByIdAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists existing <see cref="V1Correlation"/>s
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="List{T}"/> containing all existing <see cref="V1Correlation"/>s</returns>
        [OperationContract]
        Task<List<V1Correlation>> GetCorrelationsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists existing <see cref="V1Correlation"/>s
        /// </summary>
        /// <param name="query">The OData query string</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="List{T}"/> containing all existing <see cref="V1Correlation"/>s</returns>
        [OperationContract]
        Task<List<V1Correlation>> GetCorrelationsAsync(string query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the <see cref="V1Correlation"/> with the specified id
        /// </summary>
        /// <param name="id">The id of the <see cref="V1Correlation"/> to delete</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        [OperationContract]
        Task DeleteCorrelationAsync(string id, CancellationToken cancellationToken = default);

        #endregion

    }

}

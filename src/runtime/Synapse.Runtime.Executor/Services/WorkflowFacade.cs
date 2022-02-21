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
using Synapse.Integration.Events;
using Synapse.Integration.Events.WorkflowActivities;
using Synapse.Integration.Services;

namespace Synapse.Runtime.Services
{

    public class WorkflowFacade
        : IWorkflowFacade
    {

        public WorkflowFacade(WorkflowDefinition definition, V1WorkflowInstanceDto instance, ISynapseRuntimeApi synapseRuntimeApi)
        {
            this.Definition = definition;
            this.Instance = instance;
            this.SynapseRuntimeApi = synapseRuntimeApi;
        }

        public WorkflowDefinition Definition { get; }

        public V1WorkflowInstanceDto Instance { get; }

        protected ISynapseRuntimeApi SynapseRuntimeApi { get; }

        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
           await this.SynapseRuntimeApi.StartAsync(this.Instance.Id, cancellationToken);
        }

        public virtual async Task<List<V1WorkflowActivityDto>> GetActivitiesAsync(CancellationToken cancellationToken)
        {
            return await this.SynapseRuntimeApi.GetActivitiesAsync(this.Instance.Id, cancellationToken);
        }

        public virtual async Task<V1WorkflowActivityDto> CreateActivityAsync(V1WorkflowActivityType type, object? input, IDictionary<string, string>? metadata, string? parentId, CancellationToken cancellationToken)
        {
            var inputParam = input as Any;
            if (inputParam == null && input != null)
                inputParam = Any.FromObject(input);
            var metadataParam = metadata == null ? null : new NameValueCollection<string>(metadata);
            return await this.SynapseRuntimeApi.CreateActivityAsync(new() { WorkflowInstanceId = this.Instance.Id, Type = type, Input = inputParam, Metadata = metadataParam, ParentId = parentId }, cancellationToken);
        }

        public virtual async Task InitializeActivityAsync(V1WorkflowActivityDto activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            //await this.SynapseRuntimeApi.InitializeActivityAsync(activity.Id, cancellationToken);
        }

        public virtual async Task StartOrResumeActivityAsync(V1WorkflowActivityDto activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            await this.SynapseRuntimeApi.StartActivityAsync(activity.Id, cancellationToken);
        }

        public virtual async Task SuspendActivityAsync(V1WorkflowActivityDto activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            await this.SynapseRuntimeApi.SuspendActivityAsync(activity.Id, cancellationToken);
        }

        public virtual async Task FaultActivityAsync(V1WorkflowActivityDto activity, Exception ex, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));
            await this.FaultActivityAsync(activity, new ErrorDto() { Code = ex.GetType().Name.Replace("exception", string.Empty, StringComparison.OrdinalIgnoreCase), Message = ex.Message }, cancellationToken);
        }

        public virtual async Task FaultActivityAsync(V1WorkflowActivityDto activity, ErrorDto error, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            if (error == null)
                throw new ArgumentNullException(nameof(error));
            await this.SynapseRuntimeApi.FaultActivityAsync(new() { Id = activity.Id, Error = error }, cancellationToken);;
        }

        public virtual async Task CancelActivityAsync(V1WorkflowActivityDto activity, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            await this.SynapseRuntimeApi.CancelActivityAsync(activity.Id, cancellationToken);
        }

        public virtual async Task SetActivityOutputAsync(V1WorkflowActivityDto activity, object? output, CancellationToken cancellationToken = default)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            var outputParam = output as Any;
            if (outputParam == null && output != null)
                outputParam = Any.FromObject(output);
            await this.SynapseRuntimeApi.SetActivityOutputAsync(new() { Id = activity.Id, Output = outputParam }, cancellationToken);
        }

        public virtual async Task TransitionToAsync(StateDefinition state, CancellationToken cancellationToken = default)
        {
            //await this.Synapse.TransitionWorkflowInstanceToAsync(this.Instance.Id, state.Name, cancellationToken);
        }

        public virtual async Task FaultAsync(Exception ex, CancellationToken cancellationToken)
        {
            await this.SynapseRuntimeApi.FaultAsync(new() { Id = this.Instance.Id, Error = new() { Code = ex.GetType().Name.Replace("exception", string.Empty, StringComparison.InvariantCultureIgnoreCase), Message = ex.Message } }, cancellationToken);
        }

        public virtual async Task SetOutputAsync(object? output, CancellationToken cancellationToken = default)
        {
            var outputParam = output as Any;
            if (outputParam == null && output != null)
                outputParam = Any.FromObject(output);
            await this.SynapseRuntimeApi.SetOutputAsync(new() { Id = this.Instance.Id, Output = outputParam }, cancellationToken);
        }

        public virtual async Task On(V1WorkflowActivityDto activity, IV1WorkflowActivityIntegrationEvent e, CancellationToken cancellationToken = default)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            switch (e)
            {
                case V1WorkflowActivityStartedIntegrationEvent:
                case V1WorkflowActivityResumedIntegrationEvent:
                    await this.StartOrResumeActivityAsync(activity, cancellationToken);
                    break;
                case V1WorkflowActivitySuspendedIntegrationEvent:
                    await this.SuspendActivityAsync(activity, cancellationToken);
                    break;
                case V1WorkflowActivityFaultedIntegrationEvent faultedEvent:
                    await this.FaultActivityAsync(activity, faultedEvent.Error, cancellationToken);
                    break;
                case V1WorkflowActivityCancelledIntegrationEvent:
                    await this.CancelActivityAsync(activity, cancellationToken);
                    break;
                case V1WorkflowActivityCompletedIntegrationEvent completedEvent:
                    await this.SetActivityOutputAsync(activity, completedEvent.Output, cancellationToken);
                    break;
                default:
                    throw new NotSupportedException($"The specified workflow activity integration event type '{e.GetType().Name}' is not supported in this context");
            }
        }
    }

}

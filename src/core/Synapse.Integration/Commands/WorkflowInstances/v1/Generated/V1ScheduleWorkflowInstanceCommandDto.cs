﻿
/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0(the "License");
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
 */

/* -----------------------------------------------------------------------
 * This file has been automatically generated by a tool
 * -----------------------------------------------------------------------
 */

namespace Synapse.Integration.Commands.WorkflowInstances
{

	/// <summary>
	/// Represents the ICommand used to schedule the execution of an existing V1WorkflowInstance
	/// </summary>
	[DataContract]
	public partial class V1ScheduleWorkflowInstanceCommandDto
		: CommandDto
	{

		/// <summary>
		/// The id of the V1WorkflowInstance to schedule
		/// </summary>
		[DataMember(Name = "Id", Order = 1)]
		[Description("The id of the V1WorkflowInstance to schedule")]
		public virtual string Id { get; set; }

		/// <summary>
		/// The date and time at which to schedule the V1WorkflowInstance
		/// </summary>
		[DataMember(Name = "At", Order = 2)]
		[Description("The date and time at which to schedule the V1WorkflowInstance")]
		public virtual DateTime At { get; set; }

    }

}

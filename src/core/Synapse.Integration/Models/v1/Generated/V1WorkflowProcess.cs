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

namespace Synapse.Integration.Models
{

	/// <summary>
	/// Represents a process
	/// </summary>
	[DataContract]
	[Queryable]
	public partial class V1WorkflowProcess
		: Entity
	{

		/// <summary>
		/// The date and time at which the V1WorkflowProcess has exited
		/// </summary>
		[DataMember(Name = "ExitedAt", Order = 1)]
		[Description("The date and time at which the V1WorkflowProcess has exited")]
		public virtual DateTime? ExitedAt { get; set; }

		/// <summary>
		/// The logs associated to the V1WorkflowProcess
		/// </summary>
		[DataMember(Name = "Logs", Order = 2)]
		[Description("The logs associated to the V1WorkflowProcess")]
		public virtual string Logs { get; set; }

		/// <summary>
		/// The V1WorkflowProcess's exit code
		/// </summary>
		[DataMember(Name = "ExitCode", Order = 3)]
		[Description("The V1WorkflowProcess's exit code")]
		public virtual long? ExitCode { get; set; }

    }

}

﻿/*
 * Copyright © 2022-Present The Synapse Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using BlazorMonaco;

namespace Synapse.Dashboard
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMonacoEditorHelper
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="language"></param>
        /// <param name="readOnly"></param>
        /// <returns></returns>
        public Func<MonacoEditor, StandaloneEditorConstructionOptions> GetStandaloneEditorConstructionOptions(string value = "", bool readOnly = false, string language = "json");

        public Func<MonacoDiffEditor, DiffEditorConstructionOptions> GetDiffEditorConstructionOptions(bool readOnly = true);

    }
}

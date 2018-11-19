/* Copyright 2013-2015 MongoDB Inc.
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
*/

namespace NewLibCore.Data.Mongodb.Core
{
    /// <summary>
    /// Represents the read preference mode.
    /// </summary>
    public enum ReadPreferenceMode
    {
        /// <summary>
        /// Reads should be from the primary.
        /// </summary>
        Primary,

        /// <summary>
        /// Reads should be from the primary if possible, otherwise from a secondary.
        /// </summary>
        PrimaryPreferred,

        /// <summary>
        /// Reads should be from a secondary.
        /// </summary>
        Secondary,

        /// <summary>
        /// Reads should be from a secondary if possible, otherwise from the primary.
        /// </summary>
        SecondaryPreferred,

        /// <summary>
        /// Reads should be from any server that is within the latency threshold window.
        /// </summary>
        Nearest
    }
}

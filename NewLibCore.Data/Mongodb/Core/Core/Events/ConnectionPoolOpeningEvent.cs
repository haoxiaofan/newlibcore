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

using NewLibCore.Data.Mongodb.Core.Core.Clusters;
using NewLibCore.Data.Mongodb.Core.Core.Configuration;
using NewLibCore.Data.Mongodb.Core.Core.Servers;

namespace NewLibCore.Data.Mongodb.Core.Core.Events
{
    /// <preliminary/>
    /// <summary>
    /// Occurs before the pool is opened.
    /// </summary>
    public struct ConnectionPoolOpeningEvent
    {
        private readonly ServerId _serverId;
        private readonly ConnectionPoolSettings _connectionPoolSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionPoolOpeningEvent"/> struct.
        /// </summary>
        /// <param name="serverId">The server identifier.</param>
        /// <param name="connectionPoolSettings">The connection pool settings.</param>
        public ConnectionPoolOpeningEvent(ServerId serverId, ConnectionPoolSettings connectionPoolSettings)
        {
            _serverId = serverId;
            _connectionPoolSettings = connectionPoolSettings;
        }

        /// <summary>
        /// Gets the cluster identifier.
        /// </summary>
        public ClusterId ClusterId
        {
            get { return _serverId.ClusterId; }
        }

        /// <summary>
        /// Gets the connection pool settings.
        /// </summary>
        public ConnectionPoolSettings ConnectionPoolSettings
        {
            get { return _connectionPoolSettings; }
        }

        /// <summary>
        /// Gets the server identifier.
        /// </summary>
        public ServerId ServerId
        {
            get { return _serverId; }
        }
    }
}

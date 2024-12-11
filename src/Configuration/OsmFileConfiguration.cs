#region OpenPLZ API OSM - Copyright (c) STÜBER SYSTEMS GmbH
/*    
 *    OpenPLZ API 
 *    
 *    Copyright (c) STÜBER SYSTEMS GmbH
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU Affero General Public License, version 3,
 *    as published by the Free Software Foundation.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *    GNU Affero General Public License for more details.
 *
 *    You should have received a copy of the GNU Affero General Public License
 *    along with this program. If not, see <http://www.gnu.org/licenses/>.
 *
 */
#endregion

using System;

namespace OpenPlzApi.Osm
{
    /// <summary>
    /// OSM Protocol Buffer (PBF) source file configuration
    /// </summary>
    public class OsmFileConfiguration
    {
        /// <summary>
        /// Local cache of the PBF file
        /// </summary>
        public string LocalPbfFileName { get; set; }

        /// <summary>
        /// Download url of the PBF file
        /// </summary>
        public Uri RemotePbfFile { get; set; }
    }
}

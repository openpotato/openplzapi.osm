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

namespace OpenPlzApi.Osm
{
    /// <summary>
    /// Data source configuration
    /// </summary>
    public class SourcesConfiguration
    {
        /// <summary>
        /// OSM Protocol Buffer source file
        /// </summary>
        public OsmFileConfiguration Osm { get; set; }

        /// <summary>
        /// Local cache of the extracted street related OSM entities
        /// </summary>
        public string LocalStreetsSnapshotFileName { get; set; }

        /// <summary>
        /// Root folder name for relative paths
        /// </summary>
        public string RootFolderName { get; set; }
    }
}

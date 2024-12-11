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

using System.Collections.Generic;

namespace OpenPlzApi.Osm
{
    /// <summary>
    /// The OSM definition for a named area
    /// </summary>
    public class OsmArea
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OsmArea"/> class.
        /// </summary>
        public OsmArea()
        {
        }

        /// <summary>
        /// List of inner way ids
        /// </summary>
        public List<long> InnerWays { get; set; } = [];

        /// <summary>
        /// Area name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of outer way ids
        /// </summary>
        public List<long> OuterWays { get; set; } = [];
    }
}

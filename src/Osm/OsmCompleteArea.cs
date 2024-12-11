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
    /// The completed OSM definition for a named area
    /// </summary>
    public class OsmCompleteArea
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OsmCompleteArea"/> class.
        /// </summary>
        public OsmCompleteArea()
        {
        }

        /// <summary>
        /// List of inner ways
        /// </summary>
        public List<OsmCompleteWay> InnerWays { get; set; } = [];

        /// <summary>
        /// Areea name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of outer ways
        /// </summary>
        public List<OsmCompleteWay> OuterWays { get; set; } = [];
    }
}

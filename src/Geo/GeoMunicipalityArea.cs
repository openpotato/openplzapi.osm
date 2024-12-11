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
    /// A municipality area
    /// </summary>
    public class GeoMunicipalityArea : GeoArea
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OsmMunicipalityArea"/> class.
        /// </summary>
        /// <param name="osmMunicipalityArea">An OSM municipality area definition</param>
        public GeoMunicipalityArea(OsmCompleteMunicipalityArea osmMunicipalityArea)
            : base(osmMunicipalityArea)
        {
            RegionalKey = osmMunicipalityArea.RegionalKey;
        }

        /// <summary>
        /// The official regional key
        /// </summary>
        public string RegionalKey { get; set; }
    }
}

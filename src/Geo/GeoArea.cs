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

using NetTopologySuite.Geometries;

namespace OpenPlzApi.Osm
{
    /// <summary>
    /// A named area
    /// </summary>
    public abstract class GeoArea
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoArea"/> class.
        /// </summary>
        /// <param name="osmArea">An OSM area definition</param>
        public GeoArea(OsmCompleteArea osmArea)
        {
            Name = osmArea.Name.Replace(" - ", "-");
            Geometry = GeoFactory.CreateMultiPolygon(osmArea);
            ValidateAndFixGeometry();
        }

        /// <summary>
        /// The genmetry (polygon or multipolygon) of the area
        /// </summary>
        public Geometry Geometry { get; set; }

        /// <summary>
        /// Area name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Is geomatry of this area valid?
        /// </summary>
        /// <returns>TRUE, if valid</returns>
        public bool IsValid()
        {
            return (Geometry != null && Geometry.IsValid);
        }

        /// <summary>
        /// Some areas are invalid, try to fix it.
        /// </summary>
        private void ValidateAndFixGeometry()
        {
            if (Geometry != null && !Geometry.IsValid)
            {
                Geometry = NetTopologySuite.Geometries.Utilities.GeometryFixer.Fix(Geometry);
            }
        }
    }
}

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
    /// The OSM definition for a node
    /// </summary>
    public class OsmNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OsmNode"/> class.
        /// </summary>
        public OsmNode()
        {
        }

        /// <summary>
        /// Latitude
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Determines whether two object instances are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified Object is equal to the current Object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            // Compare values
            return (Latitude == ((OsmNode)obj).Latitude) && (Longitude == ((OsmNode)obj).Longitude);
        }

        /// <summary>
        /// Serves as a hash function for the entity object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Latitude, Longitude);
        }
    }
}

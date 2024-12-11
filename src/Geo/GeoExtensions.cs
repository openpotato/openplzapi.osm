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
using System.Collections.Generic;

namespace OpenPlzApi.Osm
{
    public static class OsmExtensions
    {
        public static Coordinate GetCoordinate(this OsmNode node)
        {
            return new Coordinate(node.Latitude, node.Longitude);
        }

        public static List<Coordinate> GetCoordinates(this OsmCompleteWay way)
        {
            if (way.Nodes == null)
            {
                return null;
            }

            List<Coordinate> list = [];

            for (int i = 0; i < way.Nodes.Count; i++)
            {
                list.Add(way.Nodes[i].GetCoordinate());
            }

            return list;
        }

        public static bool IsClosed(this OsmCompleteWay way)
        {
            if (way.Nodes != null && way.Nodes.Count > 1)
            {
                return way.Nodes[0].Equals(way.Nodes[way.Nodes.Count - 1]);
            }
            return false;
        }
    }
}
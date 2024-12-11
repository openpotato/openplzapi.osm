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
using OsmSharp.Geo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenPlzApi.Osm
{
    /// <summary>
    /// Factory class for creating line string and polygon geometries.
    /// </summary>
    public static class GeoFactory
    {
        /// <summary>
        /// Creates a geometric line string from an osm way definition
        /// </summary>
        /// <param name="way">An osm way definition</param>
        /// <returns>The created line string</returns>
        public static Geometry CreateLineString(OsmCompleteWay way)
        {
            var coordinates = new List<Coordinate>(way.Nodes.Count);

            foreach (var node in way.Nodes)
            {
                coordinates.Add(node.GetCoordinate());
            }

            return new LineString(coordinates.ToArray());
        }

        /// <summary>
        /// Creates a polygon or multipolygon from an osm area definition
        /// </summary>
        /// <param name="area">An osm area definition</param>
        /// <returns>The created polygon or multipolygon</returns>
        /// <remarks>
        /// This implementation is an adapted version from the OsmSharp project, which in turn is loosely 
        /// based on the following algorithm: https://wiki.openstreetmap.org/wiki/Relation:multipolygon/Algorithm
        /// </remarks>
        public static Geometry CreateMultiPolygon(OsmCompleteArea area)
        {
            // Build lists of outer and inner ways (outer=true)
            var ways = new List<KeyValuePair<bool, OsmCompleteWay>>(); 

            // Add outer ways
            foreach (var way in area.OuterWays)
            {
                ways.Add(new KeyValuePair<bool, OsmCompleteWay>(true, way));
            }

            // Add inner ways
            foreach (var way in area.InnerWays)
            {
                ways.Add(new KeyValuePair<bool, OsmCompleteWay>(false, way));
            }

            // Recusively try to assign the rings
            if (!AssignRings(ways, out var rings))
            {
                throw new InvalidOperationException($"Ring assignment failed: invalid multipolygon area [{area.Name}] detected!");
            }

            // Group the rings and create a polygon or multipolygon
            return GroupRings(rings);
        }

        /// <summary>
        /// Creates a new lineair ring from the given way and updates the assigned flags array.
        /// </summary>
        private static bool AssignRing(List<KeyValuePair<bool, OsmCompleteWay>> ways, int wayIndex, bool[] assignedFlags, out LinearRing ring)
        {
            assignedFlags[wayIndex] = true;
            List<Coordinate> coordinates;

            if (ways[wayIndex].Value.IsClosed())
            {
                // The way is closed
                coordinates = ways[wayIndex].Value.GetCoordinates();
            }
            else
            {
                // The way is open
                var roleFlag = ways[wayIndex].Key;

                // Complete the ring
                var nodes = new List<OsmNode>(ways[wayIndex].Value.Nodes);

                if (CompleteRing(ways, assignedFlags, nodes, roleFlag))
                {
                    // The ring was completed!
                    coordinates = new List<Coordinate>(nodes.Count);
                    foreach (var node in nodes)
                    {
                        coordinates.Add(node.GetCoordinate());
                    }
                }
                else
                {
                    // Oeps, assignment failed: backtrack again!
                    assignedFlags[wayIndex] = false;
                    ring = null;
                    return false;
                }
            }
            ring = new LinearRing(coordinates.ToArray());
            return true;
        }

        /// <summary>
        /// Tries to extract all rings from the given ways.
        /// </summary>
        private static bool AssignRings(List<KeyValuePair<bool, OsmCompleteWay>> ways, out List<KeyValuePair<bool, LinearRing>> rings)
        {
            return AssignRings(ways, new bool[ways.Count], out rings);
        }

        /// <summary>
        /// Assigns rings to the unassigned ways.
        /// </summary>
        private static bool AssignRings(List<KeyValuePair<bool, OsmCompleteWay>> ways, bool[] assignedFlags, out List<KeyValuePair<bool, LinearRing>> rings)
        {
            var assigned = false;
            for (var i = 0; i < ways.Count; i++)
            {
                if (!assignedFlags[i])
                {
                    assigned = true;

                    if (AssignRing(ways, i, assignedFlags, out LinearRing ring))
                    {
                        if (AssignRings(ways, assignedFlags, out List<KeyValuePair<bool, LinearRing>> otherRings))
                        {
                            rings = otherRings;
                            rings.Add(new KeyValuePair<bool, LinearRing>(ways[i].Key, ring));
                            return true;
                        }
                    }
                }
            }
            rings = new List<KeyValuePair<bool, LinearRing>>();
            return !assigned;
        }

        /// <summary>
        /// Checks if a ring is not contained by any other unused ring.
        /// </summary>
        private static bool CheckUncontained(List<KeyValuePair<bool, LinearRing>> rings, bool[][] containsFlags, bool[] used, int ringIdx)
        {
            for (var i = 0; i < rings.Count; i++)
            {
                if (i != ringIdx && !used[i] && containsFlags[i][ringIdx])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Completes an uncompleted ring.
        /// </summary>
        private static bool CompleteRing(List<KeyValuePair<bool, OsmCompleteWay>> ways, bool[] assignedFlags, List<OsmNode> nodes, bool? role)
        {
            for (var idx = 0; idx < ways.Count; idx++)
            {
                // Way not assigned
                if (!assignedFlags[idx])
                { 
                    var wayEntry = ways[idx];
                    var nextWay = wayEntry.Value;

                    // Only try matching roles if the role has been set.
                    if (!role.HasValue || wayEntry.Key == role.Value)
                    { 
                        List<OsmNode> nextNodes = null;

                        if (nodes[^1].Equals(nextWay.Nodes[0]))
                        {
                            // Last node of the previous way is the first node of the next way.
                            nextNodes = nextWay.Nodes.GetRange(1, nextWay.Nodes.Count - 1);
                            assignedFlags[idx] = true;
                        }

                        else if (nodes[^1].Equals(nextWay.Nodes[^1]))
                        {
                            // Last node of the previous way is the last node of the next way.
                            nextNodes = nextWay.Nodes.GetRange(0, nextWay.Nodes.Count - 1);
                            nextNodes.Reverse();
                            assignedFlags[idx] = true;
                        }

                        // Add the next nodes if any
                        if (assignedFlags[idx])
                        
                        {   // Yep, way was assigned!
                            nodes.AddRange(nextNodes);
                            
                            if (nodes[nodes.Count - 1].Equals(nodes[0]))
                            {   // Yes! A closed ring was found!
                                return true;
                            }
                            else
                            { 
                                // Noo! ring not closed yet!
                                if (CompleteRing(ways, assignedFlags, nodes, role))
                                { 
                                    // Yes! a complete ring was found
                                    return true;
                                }
                                else
                                { 
                                    // Damn complete ring not found. backtrack people!
                                    assignedFlags[idx] = false;
                                    nodes.RemoveRange(nodes.Count - nextNodes.Count, nextNodes.Count);
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Groups the rings into polygons.
        /// </summary>
        private static Geometry GroupRings(List<KeyValuePair<bool, LinearRing>> rings)
        {
            Geometry geometry = null;
            var containsFlags = new bool[rings.Count][]; // means [x] contains [y]
            
            for (var x = 0; x < rings.Count; x++)
            {
                var xPolygon = new Polygon(rings[x].Value); 
                containsFlags[x] = new bool[rings.Count];
            
                for (var y = 0; y < rings.Count; y++)
                {
                    var yPolygon = new Polygon(rings[y].Value); 
                    try
                    {
                        containsFlags[x][y] = xPolygon.Contains(yPolygon);
                    }
                    catch (TopologyException)
                    {
                        return null;
                    }
                }
            }
            
            var used = new bool[rings.Count];
            List<Polygon> multiPolygon = null;
            
            while (used.Contains(false))
            {   
                // Select a ring not contained by any other.
                LinearRing outer = null;
                int outerIdx = -1;
                for (int idx = 0; idx < rings.Count; idx++)
                {
                    if (!used[idx] && CheckUncontained(rings, containsFlags, used, idx))
                    { 
                        // This ring is not contained in any other used rings.
                        if (!rings[idx].Key)
                        {
                            throw new InvalidOperationException("Invalid multipolygon relation: an 'inner' ring was detected without an 'outer'.");
                        }
                        outerIdx = idx;
                        outer = rings[idx].Value;
                        used[idx] = true;
                        break;
                    }
                }

                if (outer != null)
                { 
                    // An outer ring was found, find inner rings.
                    var inners = new List<LinearRing>();
                    
                    // Select all rings contained by inner but not by any others.
                    for (int idx = 0; idx < rings.Count; idx++)
                    {
                        if (!used[idx] && containsFlags[outerIdx][idx] && CheckUncontained(rings, containsFlags, used, idx))
                        {
                            inners.Add(rings[idx].Value);
                            used[idx] = true;
                        }
                    }

                    var unused = !used.Contains(false);
            
                    if (multiPolygon == null && inners.Count == 0 && unused)
                    { 
                        // There is just one lineair ring.
                        geometry = new Polygon(outer);
                        break;
                    }
                    else if (multiPolygon == null && unused)
                    {   
                        // There is just one polygon.
                        geometry = new Polygon(outer, inners.ToArray()); 
                        break;
                    }
                    else
                    { 
                        // there have to be other polygons.
                        if (multiPolygon == null)
                        {
                            multiPolygon = new List<Polygon>();
                        }
                        multiPolygon.Add(new Polygon(outer, inners.ToArray()));
                        geometry = new MultiPolygon(multiPolygon.ToArray());
                    }
                }
                else
                {
                    // unused rings left but they cannot be designated as 'outer'.
                    throw new InvalidOperationException("Invalid multipolygon relation: Unassigned rings left.");
                }
            }
            return geometry;
        }
    }
}
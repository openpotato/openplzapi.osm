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

using Enbrea.Konsoli;
using OsmSharp;
using OsmSharp.Streams;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OpenPlzApi.Osm
{
    /// <summary>
    /// Extracts all streets relevant osm entites from a given OSM PBF file.
    /// </summary>
    public partial class OsmStreetsSnapshot
    {
        private OsmStreetsSnapshotStorage _storage = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="OsmStreetsSnapshot"/> class.
        /// </summary>
        public OsmStreetsSnapshot()
        {
        }

        /// <summary>
        /// Extracts all OSM relations, paths and nodes relevant for road extraction.
        /// </summary>
        /// <param name="pbfFile">The OSM protocol buffer file</param>
        /// <param name="consoleWriter">Progress reporting</param>
        /// <param name="cancellationToken">A cancellation token</param>
        public void CreateSnapshot(FileInfo pbfFile, ConsoleWriter consoleWriter, CancellationToken cancellationToken)
        {
            LoadUsedRelations(pbfFile, consoleWriter, cancellationToken);
            LoadUsedWays(pbfFile, consoleWriter, cancellationToken);
            LoadUsedNodes(pbfFile, consoleWriter, cancellationToken);
        }

        /// <summary>
        /// Iterates over the list of OSM borough areas
        /// </summary>
        /// <returns>An enumerator of <see cref="OsmCompleteBoroughArea"/> instances</returns>
        public IEnumerable<OsmCompleteBoroughArea> ListOfBoroughAreas()
        {
            foreach (var osmBoroughArea in _storage.BoroughAreas)
            {
                var osmCompleteBoroughArea = new OsmCompleteBoroughArea()
                {
                    Name = osmBoroughArea.Name,
                    RegionalKey = osmBoroughArea.RegionalKey,
                };

                if (CompleteWays(osmCompleteBoroughArea, osmBoroughArea))
                {
                    yield return osmCompleteBoroughArea;
                }
            }
        }

        /// <summary>
        /// Iterates over the list of OSM municipality areas
        /// </summary>
        /// <returns>An enumerator of <see cref="OsmCompleteMunicipalityArea"/> instances</returns>
        public IEnumerable<OsmCompleteMunicipalityArea> ListOfMunicipalityAreas()
        {
            foreach (var osmMunicipalityArea in _storage.MunicipalityAreas)
            {
                var osmCompleteMunicipalityArea = new OsmCompleteMunicipalityArea()
                {
                    Name = osmMunicipalityArea.Name,
                    RegionalKey = osmMunicipalityArea.RegionalKey,
                };

                if (CompleteWays(osmCompleteMunicipalityArea, osmMunicipalityArea))
                {
                    yield return osmCompleteMunicipalityArea;
                }
            }
        }

        /// <summary>
        /// Iterates over the list of OSM postal code areas
        /// </summary>
        /// <returns>An enumerator of <see cref="OsmCompletePostalCodeArea"/> instances</returns>
        public IEnumerable<OsmCompletePostalCodeArea> ListOfPostalCodeAreas()
        {
            foreach (var osmPostalCodeArea in _storage.PostalCodeAreas)
            {
                var osmCompletePostalCodeArea = new OsmCompletePostalCodeArea()
                {
                    Name = osmPostalCodeArea.Name,
                    PostalCode = osmPostalCodeArea.PostalCode,
                };

                if (CompleteWays(osmCompletePostalCodeArea, osmPostalCodeArea))
                {
                    yield return osmCompletePostalCodeArea;
                }
            }
        }

        /// <summary>
        /// Iterates over the list of OSM streets
        /// </summary>
        /// <returns>An enumerator of <see cref="OsmCompleteStreet"/> instances</returns>
        public IEnumerable<OsmCompleteStreet> ListOfStreets()
        {
            foreach (var osmStreet in _storage.Streets)
            {
                var osmCompleteStreet = new OsmCompleteStreet()
                {
                    Name = osmStreet.Name
                };

                if (CompleteNodes(osmCompleteStreet, osmStreet))
                {
                    yield return osmCompleteStreet;
                }
            }
        }

        /// <summary>
        /// Iterates over the list of OSM suburb areas
        /// </summary>
        /// <returns>An enumerator of <see cref="OsmCompleteSuburbArea"/> instances</returns>
        public IEnumerable<OsmCompleteSuburbArea> ListOfSuburbAreas()
        {
            foreach (var osmSuburbArea in _storage.SuburbAreas)
            {
                var osmCompleteSuburbArea = new OsmCompleteSuburbArea()
                {
                    Name = osmSuburbArea.Name,
                    RegionalKey = osmSuburbArea.RegionalKey,
                };

                if (CompleteWays(osmCompleteSuburbArea, osmSuburbArea))
                {
                    yield return osmCompleteSuburbArea;
                }
            }
        }

        /// <summary>
        /// Loads the extracted OSM entities from a json file.
        /// </summary>
        /// <param name="jsonFile">The json file</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A task representing an asynchronous operation</returns>
        public async Task LoadFromJsonFile(FileInfo jsonFile, CancellationToken cancellationToken)
        {
            _storage = await OsmStreetsSnapshotStorage.Load(jsonFile, cancellationToken);
        }

        /// <summary>
        /// Saves the extracted OSM entities to a json file.
        /// </summary>
        /// <param name="jsonFile">The json file</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A task representing an asynchronous operation</returns>
        public async Task SaveToJsonFile(FileInfo jsonFile, CancellationToken cancellationToken)
        {
            await _storage.Save(jsonFile, cancellationToken);
        }
        
        [GeneratedRegex("^(\\?|\\+|-|_|\\d*|\\(.*\\))$")]
        private static partial Regex BadWayNameRegex();

        private bool CompleteNodes(OsmCompleteWay completeWay, OsmWay way)
        {
            foreach (var nodeId in way.Nodes)
            {
                if (_storage.Nodes[nodeId] != null)
                {
                    completeWay.Nodes.Add(_storage.Nodes[nodeId]);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private bool CompleteWays(OsmCompleteArea completeArea, OsmArea area)
        {
            foreach (var wayId in area.OuterWays)
            {
                if (_storage.Ways[wayId] != null)
                {
                    var completeWay = new OsmCompleteWay();

                    if (CompleteNodes(completeWay, _storage.Ways[wayId]))
                    {
                        completeArea.OuterWays.Add(completeWay);

                        // The crossing of osm ways leads sometimes to the fact that multi polygons cannot be created.
                        // In this case we have to update the osm data a little bit, until the original osm dataset has
                        // been optimized.
                        if ((area.Name == "Pfullingen") && ((wayId == 328070220) || (wayId == 206313961)))
                        {
                            var idx = completeWay.Nodes.FindIndex(x => x.Latitude == 48.4556334);
                            if (idx != -1)
                            {
                                completeWay.Nodes[idx] = new OsmNode() { Latitude = 48.4556335, Longitude = 9.2800752 };
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            foreach (var wayId in area.InnerWays)
            {
                if (_storage.Ways[wayId] != null)
                {
                    var completeWay = new OsmCompleteWay();

                    if (CompleteNodes(completeWay, _storage.Ways[wayId]))
                    {
                        completeArea.InnerWays.Add(completeWay);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private void LoadUsedNodes(FileInfo pbfFile, ConsoleWriter consoleWriter, CancellationToken cancellationToken)
        {
            consoleWriter.StartProgress($"Load osm nodes from {pbfFile.Name}...");

            using var osmFileStream = pbfFile.OpenRead();
            using var osmSource = new PBFOsmStreamSource(osmFileStream);

            var osmGeoCount = 0;
            var osmNodesCount = 0;

            foreach (var osmGeo in osmSource)
            {
                if (osmGeo.Visible is null || osmGeo.Visible == true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (osmGeo is Node pbfNode)
                    {
                        if (_storage.Nodes.ContainsKey((long)pbfNode.Id))
                        {
                            var osmNode = new OsmNode()
                            {
                                Latitude = (double)pbfNode.Latitude,
                                Longitude = (double)pbfNode.Longitude
                            };
                            _storage.Nodes[(long)pbfNode.Id] = osmNode;
                            osmNodesCount++;
                        }
                    }
                    if (osmGeoCount++ % 10000 == 0) consoleWriter?.ContinueProgress("{0}/{1}", osmNodesCount, osmGeoCount++);
                }
            }
            consoleWriter?.FinishProgress("{0}/{1}", osmNodesCount, osmGeoCount);
        }
        
        private void LoadUsedRelations(FileInfo pbfFile, ConsoleWriter consoleWriter, CancellationToken cancellationToken)
        {
            consoleWriter.StartProgress($"Load osm relations from {pbfFile.Name}...");

            using var osmFileStream = pbfFile.OpenRead();
            using var osmSource = new PBFOsmStreamSource(osmFileStream);

            var osmGeoCount = 0;
            var osmRelationsCount = 0;

            foreach (var osmGeo in osmSource)
            {
                if (osmGeo.Visible is null || osmGeo.Visible == true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (osmGeo is Relation pbfRelation)
                    {
                        if (pbfRelation.Tags != null &&
                            pbfRelation.Tags.ContainsKey("postal_code") &&
                            pbfRelation.Tags["postal_code"].Length == 5 &&
                            pbfRelation.Tags.ContainsKey("note") &&
                            pbfRelation.Tags.Contains("boundary", "postal_code") &&
                            pbfRelation.Tags.Contains("type", "boundary"))
                        {
                            var osmPostalCodeArea = new OsmPostalCodeArea()
                            {
                                PostalCode = pbfRelation.Tags["postal_code"],
                                Name = pbfRelation.Tags["note"].Substring(6)
                            };
                            foreach (var pbfMember in pbfRelation.Members)
                            {
                                if (pbfMember.Type == OsmGeoType.Way)
                                {
                                    if (pbfMember.Role == "inner")
                                    {
                                        osmPostalCodeArea.InnerWays.Add(pbfMember.Id);
                                    }
                                    else
                                    {
                                        osmPostalCodeArea.OuterWays.Add(pbfMember.Id);
                                    }
                                    _storage.Ways.TryAdd(pbfMember.Id, null);
                                }
                            }
                            _storage.PostalCodeAreas.Add(osmPostalCodeArea);
                            osmRelationsCount++;
                        }
                        else if (pbfRelation.Tags != null &&
                            pbfRelation.Tags.ContainsKey("name") &&
                            pbfRelation.Tags.ContainsKey("de:amtlicher_gemeindeschluessel") &&
                            pbfRelation.Tags["de:amtlicher_gemeindeschluessel"].Length == 8 &&
                            pbfRelation.Tags.Contains("boundary", "administrative") &&
                            !pbfRelation.Tags.Contains("admin_level", "9"))
                        {
                            var osmMunicipality = new OsmMunicipalityArea()
                            {
                                Name = pbfRelation.Tags.ContainsKey("name:de") ? pbfRelation.Tags["name:de"] : pbfRelation.Tags["name"],
                                RegionalKey = pbfRelation.Tags["de:amtlicher_gemeindeschluessel"]
                            };

                            foreach (var pbfMember in pbfRelation.Members)
                            {
                                if (pbfMember.Type == OsmGeoType.Way)
                                {
                                    if (pbfMember.Role == "inner")
                                    {
                                        osmMunicipality.InnerWays.Add(pbfMember.Id);
                                    }
                                    else
                                    {
                                        osmMunicipality.OuterWays.Add(pbfMember.Id);
                                    }
                                    _storage.Ways.TryAdd(pbfMember.Id, null);
                                }
                            }
                            _storage.MunicipalityAreas.Add(osmMunicipality);
                            osmRelationsCount++;
                        }
                        else if (pbfRelation.Tags != null &&
                            pbfRelation.Tags.ContainsKey("name") &&
                            pbfRelation.Tags.Contains("admin_level", "9") &&
                            pbfRelation.Tags.Contains("boundary", "administrative"))
                        {
                            var osmBoroughArea = new OsmBoroughArea()
                            {
                                Name = pbfRelation.Tags.ContainsKey("name:de") ? pbfRelation.Tags["name:de"] : pbfRelation.Tags["name"],
                            };

                            foreach (var pbfMember in pbfRelation.Members)
                            {
                                if (pbfMember.Type == OsmGeoType.Way)
                                {
                                    if (pbfMember.Role == "inner")
                                    {
                                        osmBoroughArea.InnerWays.Add(pbfMember.Id);
                                    }
                                    else
                                    {
                                        osmBoroughArea.OuterWays.Add(pbfMember.Id);
                                    }
                                    _storage.Ways.TryAdd(pbfMember.Id, null);
                                }
                            }
                            _storage.BoroughAreas.Add(osmBoroughArea);
                            osmRelationsCount++;
                        }
                        else if (pbfRelation.Tags != null &&
                            pbfRelation.Tags.ContainsKey("name") &&
                            pbfRelation.Tags.Contains("admin_level", "10") &&
                            pbfRelation.Tags.Contains("boundary", "administrative"))
                        {
                            var osmSuburb = new OsmSuburbArea()
                            {
                                Name = pbfRelation.Tags.ContainsKey("name:de") ? pbfRelation.Tags["name:de"] : pbfRelation.Tags["name"],
                            };

                            foreach (var pbfMember in pbfRelation.Members)
                            {
                                if (pbfMember.Type == OsmGeoType.Way)
                                {
                                    if (pbfMember.Role == "inner")
                                    {
                                        osmSuburb.InnerWays.Add(pbfMember.Id);
                                    }
                                    else
                                    {
                                        osmSuburb.OuterWays.Add(pbfMember.Id);
                                    }
                                    _storage.Ways.TryAdd(pbfMember.Id, null);
                                }
                            }
                            _storage.SuburbAreas.Add(osmSuburb);
                            osmRelationsCount++;
                        }
                    }
                    if (osmGeoCount++ % 10000 == 0) consoleWriter?.ContinueProgress("{0}/{1}", osmRelationsCount, osmGeoCount++);
                }
            }
            consoleWriter.FinishProgress("{0}/{1}", osmRelationsCount, osmGeoCount);
        }

        private void LoadUsedWays(FileInfo pbfFile, ConsoleWriter consoleWriter, CancellationToken cancellationToken)
        {
            consoleWriter.StartProgress($"Load osm ways from {pbfFile.Name}...");

            using var osmFileStream = pbfFile.OpenRead();
            using var osmSource = new PBFOsmStreamSource(osmFileStream);

            var osmGeoCount = 0;
            var osmWaysCount = 0;

            foreach (var osmGeo in osmSource)
            {
                if (osmGeo.Visible is null || osmGeo.Visible == true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (osmGeo is Way pbfWay)
                    {
                        if (pbfWay.Tags != null && 
                            pbfWay.Tags.ContainsKey("name") &&
                            !pbfWay.Tags.Contains("access", "private") &&
                            !pbfWay.Tags.Contains("access", "forestry") &&
                            !pbfWay.Tags.Contains("access", "military") &&
                            !BadWayNameRegex().IsMatch(pbfWay.Tags["name"]) &&
                        (
                            pbfWay.Tags.Contains("place", "square") ||
                            pbfWay.Tags.Contains("leisure", "park") ||
                            pbfWay.Tags.Contains("highway", "primary") ||
                            pbfWay.Tags.Contains("highway", "secondary") ||
                            pbfWay.Tags.Contains("highway", "tertiary") ||
                            pbfWay.Tags.Contains("highway", "unclassified") ||
                            pbfWay.Tags.Contains("highway", "residential") ||
                            pbfWay.Tags.Contains("highway", "living_street") ||
                            pbfWay.Tags.Contains("highway", "footway") ||
                            pbfWay.Tags.Contains("highway", "road") ||
                            pbfWay.Tags.Contains("highway", "pedestrian") ||
                            (
                                pbfWay.Tags.Contains("highway", "track") && pbfWay.Tags.Contains("tracktype", "grade1")
                            ) ||
                            (
                                pbfWay.Tags.Contains("highway", "service") && pbfWay.Tags.Contains("service", "alley")
                            )
                        ))
                        {
                            var osmStreet = new OsmStreet()
                            {
                                Name = pbfWay.Tags.ContainsKey("name:de") ? pbfWay.Tags["name:de"] : pbfWay.Tags["name"]
                            };

                            foreach (var id in pbfWay.Nodes)
                            {
                                osmStreet.Nodes.Add(id);
                                _storage.Nodes.TryAdd(id, null);
                            }

                            _storage.Streets.Add(osmStreet);
                        }

                        if (_storage.Ways.ContainsKey((long)pbfWay.Id))
                        {
                            var osmWay = new OsmWay();

                            foreach (var id in pbfWay.Nodes)
                            {
                                osmWay.Nodes.Add(id);
                                _storage.Nodes.TryAdd(id, null);
                            }

                            _storage.Ways[(long)pbfWay.Id] = osmWay;
                            osmWaysCount++;
                        }
                    }
                    if (osmGeoCount++ % 10000 == 0) consoleWriter?.ContinueProgress("{0}/{1}", osmWaysCount, osmGeoCount++);
                }
            }
            consoleWriter.FinishProgress("{0}/{1}", osmWaysCount, osmGeoCount);
        }
    }
}

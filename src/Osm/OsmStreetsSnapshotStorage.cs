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
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenPlzApi.Osm
{
    /// <summary>
    /// Storage class for OSM streets snapshot
    /// </summary>
    public class OsmStreetsSnapshotStorage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OsmStreetsSnapshotStorage"/> class.
        /// </summary>
        public OsmStreetsSnapshotStorage()
        {
        }

        /// <summary>
        /// List of OSM areas representing boroughs
        /// </summary>
        public List<OsmBoroughArea> BoroughAreas { get; set; } = [];

        /// <summary>
        /// List of OSM areas representing municipalities
        /// </summary>
        public List<OsmMunicipalityArea> MunicipalityAreas { get; set; } = [];

        /// <summary>
        /// List of OSM nodes
        /// </summary>
        public Dictionary<long, OsmNode> Nodes { get; set; } = [];

        /// <summary>
        /// List of OSM areas representing postalcode areas
        /// </summary>
        public List<OsmPostalCodeArea> PostalCodeAreas { get; set; } = [];

        /// <summary>
        /// List of OSM ways representing streets
        /// </summary>
        public List<OsmStreet> Streets { get; set; } = [];

        /// <summary>
        /// List of OSM areas representing suburbs
        /// </summary>
        public List<OsmSuburbArea> SuburbAreas { get; set; } = [];
        
        /// <summary>
        /// List of OSM ways
        /// </summary>
        public Dictionary<long, OsmWay> Ways { get; set; } = [];

        /// <summary>
        /// Creates a new <see cref="OsmStreetsSnapshotStorage"/> instance by reading OSM relations, paths and nodes from a json file.
        /// </summary>
        /// <param name="jsonFile">The json file</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The new <see cref="OsmStreetsSnapshotStorage"/> instance</returns>
        static public async Task<OsmStreetsSnapshotStorage> Load(FileInfo jsonFile, CancellationToken cancellationToken)
        {
            await using var createStream = jsonFile.OpenRead();
            return await JsonSerializer.DeserializeAsync<OsmStreetsSnapshotStorage>(createStream, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Writes all OSM relations, paths and nodes to a json file
        /// </summary>
        /// <param name="jsonFile">The json file</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A task representing an asynchronous operation</returns>
        public async Task Save(FileInfo jsonFile, CancellationToken cancellationToken)
        {
            await using var createStream = jsonFile.Create();
            await JsonSerializer.SerializeAsync(createStream, this, cancellationToken: cancellationToken);
        }
    }
}

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

using Enbrea.Csv;
using Enbrea.Konsoli;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenPlzApi.Osm
{
    /// <summary>
    /// The export manager where everything comes together
    /// </summary>
    public class ExportManager
    {
        private readonly AppConfiguration _appConfiguration;
        private readonly DirectoryInfo _csvFolder;
        private readonly HashSet<CsvStreet> _csvStreets = [];
        private readonly List<GeoBoroughArea> _geoBoroughs = [];
        private readonly List<GeoMunicipalityArea> _geoMunicipalities = [];
        private readonly List<GeoPostalCodeArea> _geoPostalCodeAreas = [];
        private readonly List<GeoStreet> _geoStreets = [];
        private readonly List<GeoSuburbArea> _geoSuburbs = [];
        private readonly IDownloadClient _httpClient;
        private readonly ConsoleWriter _consoleWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportManager"/> class.
        /// </summary>
        /// <param name="appConfiguration">Configuration data</param>
        public ExportManager(AppConfiguration appConfiguration, DirectoryInfo csvFolder)
        {
            _appConfiguration = appConfiguration;
            _csvFolder = csvFolder;
            _httpClient = DownloadClientFactory.CreateClient();
            _consoleWriter = ConsoleWriterFactory.CreateConsoleWriter(ProgressUnit.Count);
        }

        /// <summary>
        /// Executes the data import, data processing and data export to csv
        /// </summary>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try 
            {
                _consoleWriter.Caption($"Import OSM data and build streets");

                _geoPostalCodeAreas.Clear();
                _geoMunicipalities.Clear();
                _geoStreets.Clear();
                _geoSuburbs.Clear();
                _geoBoroughs.Clear();

                await DownloadPbf(
                    _appConfiguration.Sources.Osm.RemotePbfFile, 
                    new FileInfo(Path.Combine(_appConfiguration.Sources.RootFolderName, _appConfiguration.Sources.Osm.LocalPbfFileName)), 
                    cancellationToken);

                await ImportFromPbf(
                    new FileInfo(Path.Combine(_appConfiguration.Sources.RootFolderName, _appConfiguration.Sources.Osm.LocalPbfFileName)),
                    new FileInfo(Path.Combine(_appConfiguration.Sources.RootFolderName, _appConfiguration.Sources.LocalStreetsSnapshotFileName)),
                    cancellationToken);

                BuildStreets(cancellationToken);

                _consoleWriter.Success($"OSM data successfully imported and processed!");
                _consoleWriter.NewLine();

                _consoleWriter.Caption($"Export streets");

                await ExportStreetsToCsv(cancellationToken);

                _consoleWriter.Success($"Streets successfully exported!");
                _consoleWriter.NewLine();
            }
            catch (Exception ex)
            {
                _consoleWriter.NewLine();
                _consoleWriter.Error($"Export failed. {ex.Message}");
                throw;
            }
        }

        public async Task ExportStreetsToCsv(CancellationToken cancellationToken)
        {
            _consoleWriter.StartProgress($"Export streets to CSV...");

            // Init csv file stream
            using var textWriter = File.CreateText(Path.Combine(_csvFolder.FullName, "streets.osm.csv"));

            // Set up csv writer
            var csvWriter = new CsvTableWriter(textWriter, new CsvConfiguration { Separator = ',' });

            // Write csv headers to stream
            await csvWriter.WriteHeadersAsync(
                "Name",
                "PostalCode",
                "Locality",
                "RegionalKey",
                "Borough",
                "Suburb");

            // Write csv data to stream
            var streetCount = 0;

            foreach (var csvStreet in _csvStreets)
            {
                cancellationToken.ThrowIfCancellationRequested();

                csvWriter.SetValue("Name", csvStreet.Name);
                csvWriter.SetValue("PostalCode", csvStreet.PostalCode);
                csvWriter.SetValue("Locality", csvStreet.Locality);
                csvWriter.SetValue("RegionalKey", csvStreet.RegionalKey);
                csvWriter.SetValue("Borough", csvStreet.Borough);
                csvWriter.SetValue("Suburb", csvStreet.Suburb);

                await csvWriter.WriteAsync();

                if ((streetCount++ % 100) == 0) _consoleWriter.ContinueProgress(streetCount);

            }

            _consoleWriter.FinishProgress(streetCount);
        }

        public async Task ImportFromPbf(FileInfo pbfFile, FileInfo jsonFile, CancellationToken cancellationToken)
        {
            var osmSnapshot = new OsmStreetsSnapshot();

            if (jsonFile.Exists)
            {
                _consoleWriter.StartProgress($"Load osm streets snapshot...");

                await osmSnapshot.LoadFromJsonFile(jsonFile, cancellationToken);

                _consoleWriter.FinishProgress();
            }
            else 
            {
                osmSnapshot.CreateSnapshot(pbfFile, _consoleWriter, cancellationToken);

                _consoleWriter.StartProgress($"Save osm streets snapshot...");

                await osmSnapshot.SaveToJsonFile(jsonFile, cancellationToken);

                _consoleWriter.FinishProgress();
            }

            _consoleWriter.StartProgress($"Extract postal code areas...");

            foreach (var osmEntity in osmSnapshot.ListOfPostalCodeAreas())
            {
                var postalCodeAreas = new GeoPostalCodeArea(osmEntity);

                if (postalCodeAreas.IsValid())
                {
                    _geoPostalCodeAreas.Add(postalCodeAreas);

                    if ((_geoPostalCodeAreas.Count % 100) == 0) _consoleWriter.ContinueProgress(_geoPostalCodeAreas.Count);
                }
            }

            _consoleWriter.FinishProgress(_geoPostalCodeAreas.Count);

            _consoleWriter.StartProgress($"Extract municipalities...");

            foreach (var osmEntity in osmSnapshot.ListOfMunicipalityAreas())
            {
                var municipalityArea = new GeoMunicipalityArea(osmEntity);

                if (municipalityArea.IsValid())
                { 
                    _geoMunicipalities.Add(municipalityArea);

                    if ((_geoMunicipalities.Count % 100) == 0) _consoleWriter.ContinueProgress(_geoMunicipalities.Count);
                }
            }

            _consoleWriter.FinishProgress(_geoMunicipalities.Count);

            _consoleWriter.StartProgress($"Extract boroughs...");

            foreach (var osmEntity in osmSnapshot.ListOfBoroughAreas())
            {
                try
                {
                    var boroughArea = new GeoBoroughArea(osmEntity);

                    if (boroughArea.IsValid())
                    {
                        _geoBoroughs.Add(boroughArea);

                        if ((_geoBoroughs.Count % 100) == 0) _consoleWriter.ContinueProgress(_geoBoroughs.Count);
                    }
                }
                catch (InvalidOperationException)
                {
                    // Igonre, Probably incomplete relations from outside Germany 
                }
            }

            _consoleWriter.FinishProgress(_geoBoroughs.Count);

            _consoleWriter.StartProgress($"Extract suburbs...");

            foreach (var osmEntity in osmSnapshot.ListOfSuburbAreas())
            {
                try
                {
                    var SuburbArea = new GeoSuburbArea(osmEntity);

                    if (SuburbArea.IsValid())
                    {
                        _geoSuburbs.Add(SuburbArea);

                        if ((_geoSuburbs.Count % 100) == 0) _consoleWriter.ContinueProgress(_geoSuburbs.Count);
                    }
                }
                catch (InvalidOperationException)
                {
                    // Igonre, probably incomplete relations from outside Germany 
                }
            }

            _consoleWriter.FinishProgress(_geoSuburbs.Count);

            _consoleWriter.StartProgress($"Extract streets...");

            foreach (var osmEntity in osmSnapshot.ListOfStreets())
            {
                _geoStreets.Add(new GeoStreet(osmEntity));

                if ((_geoStreets.Count % 100) == 0) _consoleWriter.ContinueProgress(_geoStreets.Count);
            }

            _consoleWriter.FinishProgress();
        }

        private void BuildStreets(CancellationToken cancellationToken)
        {
            _consoleWriter.StartProgress($"Build street list...");

            var streetCount = 0;

            foreach (var geoStreet in _geoStreets)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var streetCreated = false;

                foreach (var geoMunicipality in _geoMunicipalities)
                {
                    try
                    {
                        if (geoStreet.Centroid.Intersects(geoMunicipality.Geometry))
                        {
                            foreach (var geoPostalCodeArea in _geoPostalCodeAreas)
                            {
                                if (geoStreet.Centroid.Intersects(geoPostalCodeArea.Geometry))
                                {
                                    var boroughFound = false;

                                    foreach (var geoBorough in _geoBoroughs)
                                    {
                                        if (geoStreet.Centroid.Intersects(geoBorough.Geometry))
                                        {
                                            var suburbFound = false;

                                            foreach (var geoSuburb in _geoSuburbs)
                                            {
                                                if (geoStreet.Centroid.Intersects(geoSuburb.Geometry))
                                                {
                                                    var added = _csvStreets.Add(new CsvStreet
                                                    {
                                                        Name = geoStreet.Name,
                                                        PostalCode = geoPostalCodeArea.PostalCode,
                                                        Locality = geoMunicipality.Name,
                                                        RegionalKey = geoMunicipality.RegionalKey,
                                                        Borough = geoBorough.Name,
                                                        Suburb = geoSuburb.Name
                                                    });

                                                    if (added) _consoleWriter.ContinueProgress("{0}/{1}", streetCount++, _geoStreets.Count);

                                                    suburbFound = true;
                                                    streetCreated = true;
                                                }
                                            }

                                            if (!suburbFound) 
                                            {
                                                var added = _csvStreets.Add(new CsvStreet
                                                {
                                                    Name = geoStreet.Name,
                                                    PostalCode = geoPostalCodeArea.PostalCode,
                                                    Locality = geoMunicipality.Name,
                                                    RegionalKey = geoMunicipality.RegionalKey,
                                                    Borough = geoBorough.Name
                                                });

                                                if (added) _consoleWriter.ContinueProgress("{0}/{1}", streetCount++, _geoStreets.Count);

                                                streetCreated = true;
                                            }

                                            boroughFound = true;
                                        }
                                    }

                                    if (!boroughFound)
                                    {
                                        var added = _csvStreets.Add(new CsvStreet
                                        {
                                            Name = geoStreet.Name,
                                            PostalCode = geoPostalCodeArea.PostalCode,
                                            Locality = geoMunicipality.Name,
                                            RegionalKey = geoMunicipality.RegionalKey
                                        });

                                        if (added) _consoleWriter.ContinueProgress("{0}/{1}", streetCount++, _geoStreets.Count);

                                        streetCreated = true;
                                    }
                                }
                                if (streetCreated) break;
                            }
                        }
                        if (streetCreated) break;
                    }
                    catch (Exception ex) { 
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            _consoleWriter.FinishProgress(streetCount);
        }

        private async Task DownloadPbf(Uri remotePbfFile, FileInfo localPbfFile, CancellationToken cancellationToken)
        {
            if (!localPbfFile.Exists)
            {
                _consoleWriter.StartProgress($"Download {localPbfFile.Name}");

                Directory.CreateDirectory(localPbfFile.DirectoryName);

                await _httpClient.DownloadAsync(remotePbfFile, localPbfFile, cancellationToken);

                _consoleWriter.FinishProgress();
            }
        }
    }
}

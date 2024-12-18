﻿#region OpenPLZ API OSM - Copyright (c) STÜBER SYSTEMS GmbH
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

using Microsoft.Extensions.Configuration;
using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace OpenPlzApi.Osm
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Console window title
            Console.Title = AssemblyInfo.GetTitle();

            // Read configuration
            var configuration = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", optional: false)
               .AddJsonFile("appsettings.Development.json", optional: true)
               .AddJsonFile("appsettings.Production.json", optional: true)
               .Build();

            // Bind configuration
            var appConfiguration = configuration.Get<AppConfiguration>();

            // Build up command line api
            var rootCommand = new RootCommand(description: "OSM Crawler")
            {
                CommandDefinitions.Extract(appConfiguration)
            };

            // Parse the incoming args and invoke the handler
            await rootCommand.InvokeAsync(args);
        }
    }
}

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

using System.CommandLine;
using System.IO;

namespace OpenPlzApi.Osm
{
    public static class CommandDefinitions
    {
        public static Command Extract(AppConfiguration appConfiguration)
        {
            var command = new Command("extract", "Extract street data from osm and write to csv file")
            {
                new Option<DirectoryInfo>(new[] { "--folder", "-f" }, "Name of csv output folder")
                {
                    IsRequired = true
                }
            };

            command.SetHandler(async (csvFolder)
                => await CommandHandlers.Extract(appConfiguration, csvFolder),
                    command.Options[0] as Option<DirectoryInfo>);

            return command;
        }
    }
}
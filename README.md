![GitHub](https://img.shields.io/github/license/openpotato/openplzapi.osm)

# OpenPLZ API OSM

This is a .NET console application that attempts to extract all German streets (including postal codes, localities and regional keys) from the [OpenStreetMap project](https://www.openstreetmap.org/). Build with [.NET 9](https://dotnet.microsoft.com/).

## Getting started

### Prerequisites

+ Clone or download this repository.
+ Open the solution file `OpenPlzApi.Osm.sln` in Visual Studio 2022.

### Configuration

+ Switch to the project `OpenPlzApi.Osm`.
+ Make a copy of the the `appsettings.json` file and rename it to `appsettings.Development.json`.
+ Exchange the content with the following JSON document and adjust the value to your needs. This configures the root folder for the downloads of the OSM PBF file for Germany.
  
  ``` json
  {
    "Sources": {
      "RootFolderName": "c:\\OpenPlzApi.Osm\\Downloads"
    }
  }
  ```

### Export data

After you have build the project, you can start it as follows to export all streets and localities to csv (please adapt the `--folder` param to your needs):

``` cmd
OpenPlzApi.Osm export --folder c:\OpenPlzApi.Osm\Export
```

This produces the file: `streets.osm.csv`.

### Import data to PostgreSQL

To create a new database table in PostgreSQL and import the csv file, run the following sql script (please adapt the `FROM` part):

``` sql
CREATE TABLE IF NOT EXISTS public."Streets"
(
    "Name" text COLLATE pg_catalog."default",
    "PostalCode" text COLLATE pg_catalog."default",
    "Locality" text COLLATE pg_catalog."default",
    "Borough" text COLLATE pg_catalog."default",
    "Suburb" text COLLATE pg_catalog."default",
    "RegionalKey" text COLLATE pg_catalog."default"
);
	
COPY "Streets"("Name", "PostalCode", "Locality", "Borough", "Suburb", "RegionalKey")
FROM 'c:\OpenPlzApi.Osm\Export\streets.osm.csv'
DELIMITER ','
CSV HEADER 
ENCODING 'UTF8';
```

### Check data in PostgreSQL

Check your data with:

``` sql
SELECT * FROM public."Streets" S ORDER BY S."Name" 
```

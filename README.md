# Denxorz.Satisfactory.Routes

[![Build Status](https://github.com/denxorz/satisfactory-routes/workflows/Build%20and%20Test/badge.svg)](https://github.com/denxorz/satisfactory-routes/actions/workflows/build-test.yml)
[![NuGet](https://img.shields.io/nuget/v/Denxorz.Satisfactory.Routes.svg)](https://www.nuget.org/packages/Denxorz.Satisfactory.Routes)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A .NET library for parsing and analyzing Satisfactory save files to extract route and station information.

## Features

- Extract the stations for trains, drones, and trucks
- Extract the routes between stations
- Extract flow/minute based on naming convention (see)

## Installation

```bash
dotnet add package Denxorz.Satisfactory.Routes
```

## Usage

```csharp
using Denxorz.Satisfactory.Routes;

var save = SaveDetails.LoadFromStream(File.OpenRead("MySave.sav"));

foreach (var station in stations)
{
    Console.WriteLine($"Station: {station.Name}");
    foreach (var cargo in station.CargoFlows)
    {
        Console.WriteLine($"\t- Cargo: {cargo.Type} {cargo.FlowPerMinute}");
    }
    foreach (var vehicle in station.Transporters)
    {
        Console.WriteLine($"\t- Vehicle: from {vehicle.From} - to {vehicle.To}");
    }
}
```

## Naming convention

This library assumes the following naming for stations:
Name [in|out ## Type][in|out ## Type]
[Name][in|out ## Type][in|out ## Type]

For example:

- My Station [in 250.2 Rubber]
- [Other station][out 6 Plastic][out 200 Coal]

## Assumptions

The current parser assumes some things. If you need other specs, let me know in an issue.

- Stations are named based on convention above
- Trains only unload at one station
- Trucks only unload at one station

## Tools and Products Used

- [Microsoft Visual Studio Community](https://www.visualstudio.com)
- [R3dByt3 SatisfactorySaveNet](https://github.com/R3dByt3/SatisfactorySaveNet)
- [JakeBayer FuzzySharp](https://github.com/JakeBayer/FuzzySharp)
- [Icons8](https://icons8.com/)
- [NuGet](https://www.nuget.org/)
- [GitHub](https://github.com/)

## Versions & Release Notes

version 1.0: First version (.NET 8)

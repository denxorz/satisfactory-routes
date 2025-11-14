using Denxorz.Satisfactory.Routes.Parsers;
using Denxorz.Satisfactory.Routes.Types;
using SatisfactorySaveNet.Abstracts.Model;

namespace Denxorz.Satisfactory.Routes;

public static class ObjectsExtensions
{
    public static SaveDetails Parse(this (List<ComponentObject> Objects, Dictionary<string, ComponentObject> ObjectsByName) objects)
        => new(objects.ParseStations(), objects.ParseUploaders(), objects.ParseFactories());

    public static List<Station> ParseStations(this (List<ComponentObject> Objects, Dictionary<string, ComponentObject> ObjectsByName) objects)
        => [.. objects.ParseTrainStations(), .. objects.ParseDroneStations(), .. objects.ParseTruckStations()];

    public static List<Station> ParseTrainStations(this (List<ComponentObject> Objects, Dictionary<string, ComponentObject> ObjectsByName) objects)
        => [.. new TrainStationParser(objects.Objects, objects.ObjectsByName).Parse()];

    public static List<Station> ParseDroneStations(this (List<ComponentObject> Objects, Dictionary<string, ComponentObject> ObjectsByName) objects)
        => [.. new DroneStationParser(objects.Objects, objects.ObjectsByName).Parse()];

    public static List<Station> ParseTruckStations(this (List<ComponentObject> Objects, Dictionary<string, ComponentObject> ObjectsByName) objects)
        => [.. new TruckStationParser(objects.Objects, objects.ObjectsByName).Parse()];

    public static List<Uploader> ParseUploaders(this (List<ComponentObject> Objects, Dictionary<string, ComponentObject> ObjectsByName) objects)
        => [.. new UploaderParser(objects.Objects, objects.ObjectsByName).Parse()];

    public static List<Factory> ParseFactories(this (List<ComponentObject> Objects, Dictionary<string, ComponentObject> ObjectsByName) objects)
        => [.. new FactoryParser(objects.Objects, objects.ObjectsByName).Parse()];

}


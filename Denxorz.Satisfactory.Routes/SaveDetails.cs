using SatisfactorySaveNet;
using SatisfactorySaveNet.Abstracts;
using SatisfactorySaveNet.Abstracts.Model;

namespace Denxorz.Satisfactory.Routes.Parsers;

public record SaveDetails(List<Station> Stations, List<Uploader> Uploaders, List<Factory> Factories)
{
    public static SaveDetails LoadFromStream(Stream stream)
    {
        ISaveFileSerializer serializer = SaveFileSerializer.Instance;
        var saveGame = serializer.Deserialize(stream);

        var objects = saveGame.Body is BodyV8 v8
            ? v8
                .Levels
                .SelectMany(l => l.Objects)
                .ToList()
            : [];

        var objectsByName = objects.ToDictionary(o => o.ObjectReference.PathName, o => o);

        return new([
            .. new TrainStationParser(objects, objectsByName).Parse(),
            .. new DroneStationParser(objects, objectsByName).Parse(),
            .. new TruckStationParser(objects, objectsByName).Parse(),
        ],
        [.. new UploaderParser(objects, objectsByName).Parse()],
        [.. new FactoryParser(objects, objectsByName).Parse()]);
    }
}


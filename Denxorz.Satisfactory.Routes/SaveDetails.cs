using Denxorz.Satisfactory.Routes.Types;
using SatisfactorySaveNet;
using SatisfactorySaveNet.Abstracts.Model;

namespace Denxorz.Satisfactory.Routes;

public record SaveDetails(
    List<Station> Stations,
    List<Uploader> Uploaders, 
    List<Factory> Factories,
    List<PowerCircuit> PowerCircuits,
    List<Resource> Resources)
{
    public static SaveDetails LoadFromStream(Stream stream)
    {
        return LoadObjectsFromStream(stream).Parse();
    }

    public static (List<ComponentObject> Objects, Dictionary<string, ComponentObject> ObjectsByName) LoadObjectsFromStream(Stream stream)
    {
        var saveGame = SaveFileSerializer.Instance.Deserialize(stream);

        var objects= saveGame.Body is BodyV8 v8
            ? v8
                .Levels
                .SelectMany(l => l.Objects)
                .ToList()
            : [];

        var objectsByName = objects
            .DistinctBy(o => o.ObjectReference.PathName)
            .ToDictionary(o => o.ObjectReference.PathName, o => o);

        return (objects, objectsByName);
    }
}

using Denxorz.Satisfactory.Routes.Types;
using SatisfactorySaveNet.Abstracts.Model;

namespace Denxorz.Satisfactory.Routes.Parsers;

public class UploaderParser(List<ComponentObject> objects, Dictionary<string, ComponentObject> objectsByName)
{
    public IEnumerable<Uploader> Parse()
    {
        return objects
            .Where(o => o.TypePath == "/Game/FactoryGame/Buildable/Factory/CentralStorage/Build_CentralStorage.Build_CentralStorage_C")
            .OfType<ActorObject>()
            .Select(t =>
            {
                var shortId = t.ObjectReference.PathName.Split("_")[^1];
                var inventory = objectsByName[t.Properties.GetObjectPathName("mStorageInventory")];
                var cargoTypes = inventory.ToCargoTypes();

                return new Uploader(
                    shortId,
                    cargoTypes,
                    t.Position.X,
                    t.Position.Y
                );
            });
    }
}


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
                var id = t.ObjectReference.PathName.ToId();
                var cargoTypes = t.Properties.GetObject(objectsByName, "mStorageInventory").ToCargoTypes();

                return new Uploader(
                    id,
                    cargoTypes,
                    t.Position.X,
                    t.Position.Y
                );
            });
    }
}


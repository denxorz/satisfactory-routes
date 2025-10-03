using Denxorz.Satisfactory.Routes.Parsers;
using Denxorz.Satisfactory.Routes.Types;

namespace Denxorz.Satisfactory.Routes.Tests.Unit;

[TestClass]
public sealed class StationTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public static SaveDetails ClassUnderTest { get; private set; }
    public static List<Station> TrainStations { get; private set; }
    public static List<Station> DroneStations { get; private set; }
    public static List<Station> TruckStations { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext _)
    {
        // Pre load some stuff. This should part of the test, but it quite heavy to do for every test.
        // Besides that, this improves readability of the tests.
        // And if it does not work, the tests will fail anyway.

        ClassUnderTest = SaveDetails.LoadFromStream(File.OpenRead("BigSave.sav"));

        TrainStations = [.. ClassUnderTest.Stations.Where(s => s.Type == "train")];
        DroneStations = [.. ClassUnderTest.Stations.Where(s => s.Type == "drone")];
        TruckStations = [.. ClassUnderTest.Stations.Where(s => s.Type == "truck")];
    }

    [TestMethod]
    public void GetsAllStations()
    {
        Assert.AreEqual(92, ClassUnderTest.Stations.Count);
    }

    [TestMethod]
    public void GetsAllStationIds()
    {
        Assert.AreEqual(92, ClassUnderTest.Stations.Where(s => !string.IsNullOrWhiteSpace(s.Id)).Distinct().Count());
    }

    [TestMethod]
    public void GetsAllStationNames()
    {
        Assert.AreEqual(92, ClassUnderTest.Stations.Count(s => !string.IsNullOrWhiteSpace(s.Name)));
    }

    [TestMethod]
    public void GetsStationTypes()
    {
        Assert.AreEqual(53, TrainStations.Count);
        Assert.AreEqual(29, DroneStations.Count);
        Assert.AreEqual(10, TruckStations.Count);
    }

    [TestMethod]
    public void GetsAllStationCoordinates()
    {
        Assert.IsFalse(ClassUnderTest.Stations.Any(s => Math.Abs(s.X) < 0.1 || Math.Abs(s.Y) < 0.1));
    }
}

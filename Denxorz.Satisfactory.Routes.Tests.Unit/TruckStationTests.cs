namespace Denxorz.Satisfactory.Routes.Tests.Unit;

[TestClass]
public sealed class TruckStationTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private static List<Station> templeStations;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [ClassInitialize]
    public static void ClassInitialize(TestContext _)
    {
        templeStations = [.. StationTests.TruckStations.Where(s => s.ShortName == "Temple")];
    }

    [TestMethod]
    public void GetsNames()
    {
        Assert.AreEqual(1, templeStations.Count);
    }

    [TestMethod]
    public void GetsIds()
    {
        Assert.AreEqual("2147349274", templeStations[0].Id);
    }

    [TestMethod]
    public void GetsCoordinates()
    {
        Assert.AreEqual(136661.88, templeStations[0].X, 0.1);
        Assert.AreEqual(-27635.63, templeStations[0].Y, 0.1);
    }

    [TestMethod]
    public void GetsLoadUnload()
    {
        Assert.IsFalse(templeStations[0].IsUnload);
        Assert.IsTrue(StationTests.TruckStations.Single(s => s.ShortName == "BoosterChurch").IsUnload);
    }

    [TestMethod]
    public void GetsCargo()
    {
        CollectionAssert.AreEqual(new List<string>() { "Coal" }, templeStations[0].CargoTypes);
        CollectionAssert.AreEqual(new List<string>() { "QuartzCrystal" }, StationTests.TruckStations.Single(s => s.ShortName == "BoosterChurch").CargoTypes);
    }
}

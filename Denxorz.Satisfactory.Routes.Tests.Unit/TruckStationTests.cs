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

    [TestMethod]
    public void GetsCargoFlowFromName()
    {
        var utilStations = StationTests.TruckStations.Where(s => s.ShortName == "UtilTowers").ToList();

        CargoFlow flow1 = utilStations[0].CargoFlows[0];
        Assert.AreEqual("Coal", flow1.Type);
        Assert.IsTrue(flow1.IsExact);
        Assert.AreEqual(30, flow1.FlowPerMinute);
        Assert.IsTrue(flow1.IsUnload);

        // Incorrect naming
        Assert.AreEqual(0, utilStations[1].CargoFlows.Count);
    }

    [TestMethod]
    public void GetsCargoFlowFromNameNotExact()
    {
        CargoFlow flow = templeStations[0].CargoFlows[0];
        Assert.AreEqual("Coal", flow.Type);
        Assert.IsFalse(flow.IsExact);
        Assert.AreEqual(1200, flow.FlowPerMinute);
        Assert.IsFalse(flow.IsUnload);
    }

    [TestMethod]
    public void GetsTransporters()
    {
        Assert.IsTrue(templeStations.All(s => s.Transporters.All(t => t.From == s.Id)));
        Assert.AreEqual("2147329195", templeStations[0].Transporters[0].Id);
        Assert.AreEqual("2147340529", templeStations[0].Transporters[0].To);
    }
}

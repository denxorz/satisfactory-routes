using Denxorz.Satisfactory.Routes.Types;

namespace Denxorz.Satisfactory.Routes.Tests.Unit;

[TestClass]
public sealed class DroneStationTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private static List<Station> egelStations;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [ClassInitialize]
    public static void ClassInitialize(TestContext _)
    {
        egelStations = [.. StationTests.DroneStations.Where(s => s.ShortName == "Egel")];
    }

    [TestMethod]
    public void GetsNames()
    {
        Assert.AreEqual(3, egelStations.Count);
    }

    [TestMethod]
    public void GetsIds()
    {
        Assert.AreEqual("2147380390", egelStations[0].Id);
        Assert.AreEqual("2147380091", egelStations[1].Id);
        Assert.AreEqual("2147398668", egelStations[2].Id);
    }

    [TestMethod]
    public void GetsCoordinates()
    {
        Assert.AreEqual(328352.88, egelStations[0].X, 0.1);
        Assert.AreEqual(-175883.8, egelStations[0].Y, 0.1);
        Assert.AreEqual(329310.53, egelStations[1].X, 0.1);
        Assert.AreEqual(-178514.94, egelStations[1].Y, 0.1);
        Assert.AreEqual(327395.22, egelStations[2].X, 0.1);
        Assert.AreEqual(-173252.66, egelStations[2].Y, 0.1);
    }

    [TestMethod]
    public void GetsLoadUnload()
    {
        Assert.IsTrue(egelStations[0].IsUnload);
        Assert.IsFalse(egelStations[1].IsUnload);
        Assert.IsTrue(egelStations[2].IsUnload);
    }

    [TestMethod]
    public void GetsCargo()
    {
        CollectionAssert.AreEqual(new List<string>() { "QuartzCrystal" }, egelStations[0].CargoTypes);
        CollectionAssert.AreEqual(new List<string>() { "SpaceElevatorPart_9" }, egelStations[1].CargoTypes);
        CollectionAssert.AreEqual(new List<string>() { "Fuel" }, egelStations[2].CargoTypes);
    }

    [TestMethod]
    public void GetsCargoFlowFromName()
    {
        CargoFlow station1Flow = egelStations[0].CargoFlows[0];
        Assert.AreEqual("QuartzCrystal", station1Flow.Type);
        Assert.IsTrue(station1Flow.IsExact);
        Assert.AreEqual(7, station1Flow.FlowPerMinute);
        Assert.IsTrue(station1Flow.IsUnload);

        CargoFlow station2Flow = egelStations[1].CargoFlows[0];
        Assert.AreEqual("SpaceElevatorPart_9", station2Flow.Type);
        Assert.IsTrue(station2Flow.IsExact);
        Assert.AreEqual(1, station2Flow.FlowPerMinute);
        Assert.IsFalse(station2Flow.IsUnload);

        CargoFlow station3Flow = egelStations[2].CargoFlows[0];
        Assert.AreEqual("Fuel", station3Flow.Type);
        Assert.IsTrue(station3Flow.IsExact);
        Assert.AreEqual(null, station3Flow.FlowPerMinute);
        Assert.IsTrue(station3Flow.IsUnload);
    }

    [TestMethod]
    public void GetsCargoFlowFromNameNotExact()
    {
        var stationUtil = StationTests.DroneStations.Where(s => s.ShortName == "UtilTowers").ToList()[1].CargoFlows[0];
        Assert.IsFalse(stationUtil.IsExact);
    }

    [TestMethod]
    public void GetsTransporters()
    {
        Assert.IsTrue(egelStations.All(s => s.Transporters.All(t => t.To == s.Id)));
        Assert.AreEqual("2147378492", egelStations[0].Transporters[0].Id);
        Assert.AreEqual("2147380390", egelStations[0].Transporters[0].To);
        CollectionAssert.AreEquivalent(Array.Empty<string>(), egelStations[1].Transporters);
        Assert.AreEqual("2147397902", egelStations[2].Transporters[0].Id);
        Assert.AreEqual("2147398668", egelStations[2].Transporters[0].To);
    }

    [TestMethod]
    public void GetsTransportersFromToDirection()
    {
        var templeStations = StationTests.DroneStations.Where(s => s.ShortName == "Temple").ToList();
        var ficsStations = StationTests.DroneStations.Where(s => s.ShortName == "Fics").ToList();

        // Egel --NucPasta--> Temple
        Assert.AreEqual(egelStations[1].Id, templeStations[1].Transporters[0].From);
        Assert.AreEqual(templeStations[1].Id, templeStations[1].Transporters[0].To);

        // Temple --Singularity--> Fics
        Assert.AreEqual(templeStations[0].Id, ficsStations[2].Transporters[0].From);
        Assert.AreEqual(ficsStations[2].Id, ficsStations[2].Transporters[0].To);
    }
}

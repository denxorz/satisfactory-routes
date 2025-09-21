namespace Denxorz.Satisfactory.Routes.Tests.Unit;

[TestClass]
public sealed class TrainStationTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private static List<Station> northStations;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [ClassInitialize]
    public static void ClassInitialize(TestContext _)
    {
        northStations = [.. StationTests.TrainStations.Where(s => s.ShortName == "The North")];
    }

    [TestMethod]
    public void GetsNames()
    {
        Assert.AreEqual(3, northStations.Count);
    }

    [TestMethod]
    public void GetsIds()
    {
        Assert.AreEqual("2145491643", northStations[0].Id);
        Assert.AreEqual("2145489503", northStations[1].Id);
        Assert.AreEqual("2145477740", northStations[2].Id);
    }

    [TestMethod]
    public void GetsCoordinates()
    {
        Assert.AreEqual(242253.03, northStations[0].X, 0.1);
        Assert.AreEqual(-307722.97, northStations[0].Y, 0.1);
        Assert.AreEqual(228053.03, northStations[1].X, 0.1);
        Assert.AreEqual(-303222.97, northStations[1].Y, 0.1);
        Assert.AreEqual(227253.03, northStations[2].X, 0.1);
        Assert.AreEqual(-301322.97, northStations[2].Y, 0.1);
    }

    [TestMethod]
    public void GetsLoadUnload()
    {
        Assert.IsTrue(northStations[0].IsUnload);
        Assert.IsTrue(northStations[1].IsUnload);
        Assert.IsFalse(northStations[2].IsUnload);
    }

    [TestMethod]
    public void GetsCargo()
    {
        CollectionAssert.AreEqual(new List<string>() { "ModularFrameHeavy" }, northStations[0].CargoTypes);
        CollectionAssert.AreEqual(new List<string>() { "AluminumCasing" }, northStations[1].CargoTypes);
        CollectionAssert.AreEqual(new List<string>() { "ModularFrameFused" }, northStations[2].CargoTypes);
    }

    [TestMethod]
    public void GetsTransporters()
    {
        Assert.AreEqual("2146789448", northStations[0].Transporters[0].Id);
        Assert.AreEqual("2145491643", northStations[0].Transporters[0].To);
        Assert.AreEqual("2147007670", northStations[0].Transporters[0].From);
        Assert.AreEqual("[FusedF] HF", northStations[0].Transporters[0].Name);

        Assert.AreEqual("2145561005", northStations[1].Transporters[0].Id);
        Assert.AreEqual("2145489503", northStations[1].Transporters[0].To);
        Assert.AreEqual("2147396352", northStations[1].Transporters[0].From);
        Assert.AreEqual("The North [AluCase]", northStations[1].Transporters[0].Name);

        Assert.AreEqual("2142856431", northStations[2].Transporters[0].Id);
        Assert.AreEqual("2146856079", northStations[2].Transporters[0].To);
        Assert.AreEqual("2145477740", northStations[2].Transporters[0].From);
        Assert.AreEqual("Egel [FF]", northStations[2].Transporters[0].Name);
    }

    [TestMethod]
    public void GetsTransportersMulti()
    {
        var damStation = StationTests.TrainStations.Where(s => s.ShortName == "Dam").ToList();
        Assert.IsTrue(damStation.All(s => s.Transporters.All(t => t.From == s.Id)));

        Assert.AreEqual("2144905638", damStation[0].Transporters[0].Id);
        Assert.AreEqual("2144922555", damStation[0].Transporters[0].To);
        Assert.AreEqual("[Rocket] AluSheet", damStation[0].Transporters[0].Name);

        Assert.AreEqual("2146472471", damStation[0].Transporters[1].Id);
        Assert.AreEqual("2146589300", damStation[0].Transporters[1].To);
        Assert.AreEqual("[BoosterChurch] AluSheet", damStation[0].Transporters[1].Name);
    }

    [TestMethod]
    public void GetsTransportersOtherStations()
    {
        var nucStations = StationTests.TrainStations.Where(s => s.ShortName == "Nuc").ToList();
        Assert.AreEqual("2147387640", nucStations[3].Transporters[0].OtherStops[0]);
    }

    [TestMethod]
    public void GetsCargoFlowFromName()
    {
        CargoFlow station1Flow = northStations[0].CargoFlows[0];
        Assert.AreEqual("ModularFrameHeavy", station1Flow.Type);
        Assert.IsTrue(station1Flow.IsExact);
        Assert.AreEqual(4.5, station1Flow.FlowPerMinute);
        Assert.IsTrue(station1Flow.IsUnload);

        CargoFlow station2Flow = northStations[1].CargoFlows[0];
        Assert.AreEqual("AluminumCasing", station2Flow.Type);
        Assert.IsTrue(station2Flow.IsExact);
        Assert.AreEqual(225, station2Flow.FlowPerMinute);
        Assert.IsTrue(station2Flow.IsUnload);

        CargoFlow station3Flow = northStations[2].CargoFlows[0];
        Assert.AreEqual("ModularFrameFused", station3Flow.Type);
        Assert.IsTrue(station3Flow.IsExact);
        Assert.AreEqual(4.5, station3Flow.FlowPerMinute);
        Assert.IsFalse(station3Flow.IsUnload);
    }

    [TestMethod]
    public void GetsCargoFlowFromNameMatchingBug()
    {
        CargoFlow station1Flow = northStations[0].CargoFlows[0];
        Assert.AreEqual("ModularFrameHeavy", station1Flow.Type);
        Assert.IsTrue(station1Flow.IsExact);
        Assert.AreEqual(4.5, station1Flow.FlowPerMinute);
        Assert.IsTrue(station1Flow.IsUnload);

        CargoFlow station2Flow = northStations[1].CargoFlows[0];
        Assert.AreEqual("AluminumCasing", station2Flow.Type);
        Assert.IsTrue(station2Flow.IsExact);
        Assert.AreEqual(225, station2Flow.FlowPerMinute);
        Assert.IsTrue(station2Flow.IsUnload);
    }

    [TestMethod]
    public void GetsCargoFlowFromNameMultiNotExact()
    {
        var stationTriangle = StationTests.TrainStations.First(s => s.ShortName == "Triangle");

        Assert.AreEqual(225, stationTriangle.CargoFlows[0].FlowPerMinute);
        Assert.AreEqual("FicsiteIngot", stationTriangle.CargoFlows[0].Type);

        Assert.AreEqual(150, stationTriangle.CargoFlows[1].FlowPerMinute);
        Assert.AreEqual("SAMIngot", stationTriangle.CargoFlows[1].Type);
    }
}

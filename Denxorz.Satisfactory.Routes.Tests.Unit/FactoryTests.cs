namespace Denxorz.Satisfactory.Routes.Tests.Unit;

[TestClass]
public sealed class FactoryTests
{
    [ClassInitialize]
    public static void ClassInitialize(TestContext _)
    {
        StationTests.LoadBigSave();
    }

    [TestMethod]
    public void GetsAllFactories()
    { 
        Assert.HasCount(3088, StationTests.ClassUnderTest.Factories);
    }

    [TestMethod]
    public void GetsAllStationCoordinates()
    {
        Assert.IsFalse(StationTests.ClassUnderTest.Factories.Any(s => Math.Abs(s.X) < 0.1 || Math.Abs(s.Y) < 0.1));
    }

    [TestMethod]
    public void GetsIds()
    {
        Assert.AreEqual("2147367273", StationTests.ClassUnderTest.Factories[1].Id);
    }

    [TestMethod]
    public void GetsType()
    {
        Assert.AreEqual("GeneratorCoal", StationTests.ClassUnderTest.Factories[1].Type);
    }

    [TestMethod]
    public void GetsPercentageProducing()
    {
        Assert.AreEqual(41, StationTests.ClassUnderTest.Factories[61].PercentageProducing);
        Assert.AreEqual(1941, StationTests.ClassUnderTest.Factories.Count(f => f.PercentageProducing > 0));
    }

    [TestMethod]
    public void GetsPowerCircuitId()
    {
        Assert.AreEqual(0, StationTests.ClassUnderTest.Factories[61].MainPowerCircuitId);
        Assert.AreEqual(6, StationTests.ClassUnderTest.Factories[61].SubPowerCircuitId);
    }
}

namespace Denxorz.Satisfactory.Routes.Tests.Unit;

[TestClass]
public sealed class FactoryTests
{
    [TestMethod]
    public void GetsAllFactories()
    {
        Assert.AreEqual(2163, StationTests.ClassUnderTest.Factories.Count);
    }

    [TestMethod]
    public void GetsAllStationCoordinates()
    {
        Assert.IsFalse(StationTests.ClassUnderTest.Factories.Any(s => Math.Abs(s.X) < 0.1 || Math.Abs(s.Y) < 0.1));
    }

    [TestMethod]
    public void GetsIds()
    {
        Assert.AreEqual("2147367273", StationTests.ClassUnderTest.Factories[0].Id);
    }

    [TestMethod]
    public void GetsType()
    {
        Assert.AreEqual("GeneratorCoal", StationTests.ClassUnderTest.Factories[0].Type);
    }

    [TestMethod]
    public void GetsPercentageProducing()
    {
        Assert.AreEqual(27, StationTests.ClassUnderTest.Factories[61].PercentageProducing);
    }

    [TestMethod]
    public void GetsPowerCircuitId()
    {
        Assert.AreEqual(26, StationTests.ClassUnderTest.Factories[61].PowerCircuitId);
    }
}

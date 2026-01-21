namespace Denxorz.Satisfactory.Routes.Tests.Unit;

[TestClass]
public sealed class PowerCircuitTests
{
    [ClassInitialize]
    public static void ClassInitialize(TestContext _)
    {
        StationTests.LoadBigSave();
    }

    [TestMethod]
    public void GetsAllPowerCircuits()
    {
        Assert.HasCount(36, StationTests.ClassUnderTest.PowerCircuits);
        Assert.HasCount(26, StationTests.ClassUnderTest.PowerCircuits.Where(c => c.ParentCircuitId is not null));
    }

    [TestMethod]
    public void GetsProperties()
    {
        Assert.AreEqual(57, StationTests.ClassUnderTest.PowerCircuits[18].Id);
        Assert.AreEqual(0, StationTests.ClassUnderTest.PowerCircuits[18].ParentCircuitId);
        Assert.AreEqual("[Portal] Alu", StationTests.ClassUnderTest.PowerCircuits[18].Name);
        Assert.AreEqual(8, StationTests.ClassUnderTest.PowerCircuits[18].Priority);
        Assert.IsTrue(StationTests.ClassUnderTest.PowerCircuits[18].IsOn);
    }
}

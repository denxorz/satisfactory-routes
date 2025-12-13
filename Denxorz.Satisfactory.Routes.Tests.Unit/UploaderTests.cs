namespace Denxorz.Satisfactory.Routes.Tests.Unit;

[TestClass]
public sealed class UploaderTests
{
    [ClassInitialize]
    public static void ClassInitialize(TestContext _)
    {
        StationTests.LoadBigSave();
    }

    [TestMethod]
    public void GetsAllUploaders()
    {
        Assert.HasCount(69, StationTests.ClassUnderTest.Uploaders);
    }

    [TestMethod]
    public void GetsAllStationCoordinates()
    {
        Assert.IsFalse(StationTests.ClassUnderTest.Uploaders.Any(s => Math.Abs(s.X) < 0.1 || Math.Abs(s.Y) < 0.1));
    }

    [TestMethod]
    public void GetsIds()
    {
        Assert.AreEqual("2147246994", StationTests.ClassUnderTest.Uploaders[0].Id);
    }

    [TestMethod]
    public void GetsCargo()
    {
        CollectionAssert.AreEqual(new List<string>() { "CopperSheet" }, StationTests.ClassUnderTest.Uploaders[0].CargoTypes);
    }
}

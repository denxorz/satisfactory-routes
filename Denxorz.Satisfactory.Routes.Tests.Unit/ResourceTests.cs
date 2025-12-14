namespace Denxorz.Satisfactory.Routes.Tests.Unit;

[TestClass]
public sealed class ResourceTests
{
    [ClassInitialize]
    public static void ClassInitialize(TestContext _)
    {
        StationTests.LoadBigSave();
    }

    [TestMethod]
    public void GetsAll()
    {
        Assert.HasCount(459, StationTests.ClassUnderTest.Resources);
    }

    [TestMethod]
    public void GetsAllCoordinates()
    {
        Assert.IsFalse(StationTests.ClassUnderTest.Resources.Any(s => Math.Abs(s.X) < 0.1 || Math.Abs(s.Y) < 0.1));
    }

    [TestMethod]
    public void GetsIds()
    {
        Assert.AreEqual("ResourceNode481", StationTests.ClassUnderTest.Resources[0].Id);
    }

    [TestMethod]
    public void GetsType()
    {
        Assert.AreEqual("Bauxite", StationTests.ClassUnderTest.Resources[0].Type);
    }

    [TestMethod]
    public void GetsExtraction()
    {
        Assert.AreEqual(0, StationTests.ClassUnderTest.Resources[0].Flow);
        Assert.AreEqual(300, StationTests.ClassUnderTest.Resources[0].Max);

        Assert.AreEqual(1200, StationTests.ClassUnderTest.Resources[12].Flow);
        Assert.AreEqual(1200, StationTests.ClassUnderTest.Resources[12].Max);

        Assert.AreEqual(67.6, StationTests.ClassUnderTest.Resources[34].Flow, 0.1);
        Assert.AreEqual(300, StationTests.ClassUnderTest.Resources[34].Max); 
    }
}
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

    [TestMethod]
    public void DetectSloop()
    {
        Assert.IsTrue(StationTests.ClassUnderTest.Factories[433].HasSloop);
        Assert.IsFalse(StationTests.ClassUnderTest.Factories[434].HasSloop);
    }

    [TestMethod]
    public void DetectShard()
    {
        Assert.AreEqual(100, StationTests.ClassUnderTest.Factories[24].Potential, 0.1);
        Assert.AreEqual(75, StationTests.ClassUnderTest.Factories[37].Potential, 0.1);
        Assert.AreEqual(250, StationTests.ClassUnderTest.Factories[34].Potential, 0.1);
    }

    [TestMethod]
    public void GetsRecipe()
    {
        Assert.AreEqual("Recipe_SuperpositionOscillator_C", StationTests.ClassUnderTest.Factories[3004].RecipeClass);
        Assert.AreEqual("Superposition Oscillator", StationTests.ClassUnderTest.Factories[3004].Recipe);
        Assert.AreEqual("DarkMatter", StationTests.ClassUnderTest.Factories[3004].Input[0].Type);
        Assert.AreEqual(6, (float)StationTests.ClassUnderTest.Factories[3004].Input[0].FlowPerMinute!, 0.1);
        Assert.AreEqual("CrystalOscillator", StationTests.ClassUnderTest.Factories[3004].Input[1].Type);
        Assert.AreEqual(1, (float)StationTests.ClassUnderTest.Factories[3004].Input[1].FlowPerMinute!, 0.1);
        Assert.AreEqual("AluminumPlate", StationTests.ClassUnderTest.Factories[3004].Input[2].Type);
        Assert.AreEqual(9, (float)StationTests.ClassUnderTest.Factories[3004].Input[2].FlowPerMinute!, 0.1);
        Assert.AreEqual("QuantumEnergy", StationTests.ClassUnderTest.Factories[3004].Input[3].Type);
        Assert.AreEqual(25, (float)StationTests.ClassUnderTest.Factories[3004].Input[3].FlowPerMinute!, 0.1);
        Assert.AreEqual("QuantumOscillator", StationTests.ClassUnderTest.Factories[3004].Output[0].Type);
        Assert.AreEqual(1, (float)StationTests.ClassUnderTest.Factories[3004].Output[0].FlowPerMinute!, 0.1);
        Assert.AreEqual("DarkEnergy", StationTests.ClassUnderTest.Factories[3004].Output[1].Type);
        Assert.AreEqual(25, (float)StationTests.ClassUnderTest.Factories[3004].Output[1].FlowPerMinute!, 0.1);
    }
}

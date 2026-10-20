namespace Workboard.Data.Tests;

public class AssemblySmokeTests
{
    [Fact]
    public void ApplicationAssemblyLoads() =>
        Assert.NotNull(typeof(Workboard.Data.AssemblyMarker).Assembly);
}



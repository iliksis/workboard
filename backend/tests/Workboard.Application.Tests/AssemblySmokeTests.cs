namespace Workboard.Application.Tests;

public class AssemblySmokeTests
{
    [Fact]
    public void ApplicationAssemblyLoads() =>
        Assert.NotNull(typeof(Workboard.Application.AssemblyMarker).Assembly);
}



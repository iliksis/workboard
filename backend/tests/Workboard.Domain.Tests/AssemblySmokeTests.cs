namespace Workboard.Domain.Tests;

public class AssemblySmokeTests
{
    [Fact]
    public void DomainAssemblyLoads() =>
        Assert.NotNull(typeof(Workboard.Domain.AssemblyMarker).Assembly);
}



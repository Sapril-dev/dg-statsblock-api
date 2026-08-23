using Xunit;

public sealed class ApiSmokeTests
{
    [Fact]
    public void ApiAssemblyLoads()
    {
        Assert.NotNull(typeof(Program).Assembly);
    }
}

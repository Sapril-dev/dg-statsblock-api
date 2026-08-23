using DunorGames.Contracts.Statblocks;

public sealed class StatblockApiContractTests
{
    [Fact]
    public void CreateRequestDoesNotAcceptServerManagedFields()
    {
        var properties = typeof(CreateStatblockRequest).GetProperties();

        Assert.DoesNotContain(properties, property => property.Name == "Id");
        Assert.DoesNotContain(properties, property => property.Name == "Metadata");
        Assert.DoesNotContain(properties, property => property.Name == "OwnerId");
    }

    [Fact]
    public void PublicResponseDoesNotExposeOwnerId()
    {
        var metadataProperties = typeof(StatblockReadMetadataDto).GetProperties();

        Assert.DoesNotContain(metadataProperties, property => property.Name == "OwnerId");
    }
}

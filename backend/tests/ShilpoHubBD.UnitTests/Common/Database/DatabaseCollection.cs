namespace ShilpoHubBD.UnitTests.Common.Database;

/// <summary>All database test classes share one throwaway database and run one at a time.</summary>
[CollectionDefinition(Name)]
public sealed class DatabaseCollection : ICollectionFixture<TestDatabaseFixture>
{
    public const string Name = "Database";
}

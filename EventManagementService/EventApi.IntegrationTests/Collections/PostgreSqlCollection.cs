using EventApi.IntegrationTests.Fixtures;

namespace EventApi.IntegrationTests.Collections;

[CollectionDefinition("PostgreSql")]
public class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
}
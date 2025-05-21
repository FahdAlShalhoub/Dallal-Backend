using Xunit;

namespace Dallal.Backend.EntityFrameworkCore;

[CollectionDefinition(BackendTestConsts.CollectionDefinitionName)]
public class BackendEntityFrameworkCoreCollection : ICollectionFixture<BackendEntityFrameworkCoreFixture>
{

}

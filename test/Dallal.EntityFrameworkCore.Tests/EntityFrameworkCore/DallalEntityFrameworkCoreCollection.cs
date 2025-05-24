using Xunit;

namespace Dallal.EntityFrameworkCore;

[CollectionDefinition(DallalTestConsts.CollectionDefinitionName)]
public class DallalEntityFrameworkCoreCollection : ICollectionFixture<DallalEntityFrameworkCoreFixture>
{

}

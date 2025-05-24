using Dallal.Samples;
using Xunit;

namespace Dallal.EntityFrameworkCore.Domains;

[Collection(DallalTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<DallalEntityFrameworkCoreTestModule>
{

}

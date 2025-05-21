using Dallal.Backend.Samples;
using Xunit;

namespace Dallal.Backend.EntityFrameworkCore.Domains;

[Collection(BackendTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<BackendEntityFrameworkCoreTestModule>
{

}

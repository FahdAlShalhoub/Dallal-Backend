using Dallal.Backend.Samples;
using Xunit;

namespace Dallal.Backend.EntityFrameworkCore.Applications;

[Collection(BackendTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<BackendEntityFrameworkCoreTestModule>
{

}

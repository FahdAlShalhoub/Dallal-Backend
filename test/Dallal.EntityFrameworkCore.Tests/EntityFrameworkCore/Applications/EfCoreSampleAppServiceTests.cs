using Dallal.Samples;
using Xunit;

namespace Dallal.EntityFrameworkCore.Applications;

[Collection(DallalTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<DallalEntityFrameworkCoreTestModule>
{

}

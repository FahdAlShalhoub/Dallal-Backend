using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Dallal.Pages;

[Collection(DallalTestConsts.CollectionDefinitionName)]
public class Index_Tests : DallalWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}

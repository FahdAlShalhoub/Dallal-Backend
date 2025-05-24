using System.Threading.Tasks;
using Dallal.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Services;

namespace Dallal;

/* Inherit your application services from this class.
 */
[ApiExplorerSettings(GroupName = "Customer")]
[Authorize("Customer")]
public class TestAppService : DallalAppService
{
    public Task<string> GetTestMessageAsync()
    {
        return Task.FromResult("Hello from TestAppService!");
    }
}

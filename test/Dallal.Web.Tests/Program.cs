using Microsoft.AspNetCore.Builder;
using Dallal;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();
builder.Environment.ContentRootPath = GetWebProjectContentRootPathHelper.Get("Dallal.Web.csproj"); 
await builder.RunAbpModuleAsync<DallalWebTestModule>(applicationName: "Dallal.Web");

public partial class Program
{
}

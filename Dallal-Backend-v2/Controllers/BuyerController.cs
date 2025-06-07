using Dallal_Backend_v2.Entities.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
[Route("[controller]")]
public class BuyerController : ControllerBase
{
    [HttpGet(Name = "GetBuyers")]
    public async Task<List<Buyer>> Get(DatabaseContext context)
    {
        var buyers = await context.Buyers.ToListAsync();
        return buyers;
    }
}

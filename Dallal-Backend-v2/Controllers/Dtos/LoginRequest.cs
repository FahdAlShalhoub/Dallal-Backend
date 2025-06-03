namespace Dallal_Backend_v2.Controllers.Dtos;

public record LoginRequest
{
   public string Email { get; init; }
   public string Password { get; init; }
}
namespace Dallal_Backend_v2.Controllers.Auth.Dtos;

public record struct UpdateUserRequest(string? FirstName, string? LastName, string? Language);
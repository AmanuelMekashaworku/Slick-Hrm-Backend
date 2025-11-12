namespace slick.Application.DTOs
{
    public record ServiceResponse(bool Success = false, object? Data = null, string? Message = null);
}
namespace slick.Application.DTOs.UserLog
{
    public class CreateUserActivityLogDto
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string? Description { get; set; }
        public string? IpAddress { get; set; }
        public string? Device { get; set; }
    }
}

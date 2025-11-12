using System;

namespace slick.Domain.Entities
{
    public class UserActivityLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = null!;
        public required AppUser AppUser { get; set; }
        public string UserName { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string? Description { get; set; }
        public string? IpAddress { get; set; }
        public string? Device { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}

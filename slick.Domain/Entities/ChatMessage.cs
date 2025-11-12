namespace slick.Domain.Entities
{
    public class ChatMessage
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}

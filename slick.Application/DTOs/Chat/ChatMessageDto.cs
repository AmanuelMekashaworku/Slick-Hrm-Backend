namespace slick.Application.DTOs.Chat
{
    public class ChatMessageDto
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
    }
}
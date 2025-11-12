namespace slick.Application.DTOs.Chat
{
    public class CreateChatMessageDto
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
}
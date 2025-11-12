using slick.Application.DTOs;
using slick.Application.DTOs.Chat;

namespace slick.Application.Services.Interfaces
{
    public interface IChatService
    {
        Task<ServiceResponse> CreateMessageAsync(CreateChatMessageDto dto);
        Task<List<ChatMessageDto>> GetPagedMessagesAsync(string? search, CancellationToken cancellationToken = default);
    }
}

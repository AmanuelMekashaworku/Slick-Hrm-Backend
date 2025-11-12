using AutoMapper;
using slick.Application.DTOs;
using slick.Application.DTOs.Chat;
using slick.Application.Services.Interfaces;
using slick.Domain.Entities;
using slick.Domain.Interfaces;
using System.Linq.Expressions;

namespace slick.Application.Services.Implementations
{
    public class ChatService(IGeneric<ChatMessage> chatRepository, IMapper mapper) : IChatService
    {
        public async Task<ServiceResponse> CreateMessageAsync(CreateChatMessageDto dto)
        {
            try
            {
                if (dto == null)
                    return new ServiceResponse(false, "Message data is null.");

                var entity = mapper.Map<ChatMessage>(dto);
                entity.CreatedDate = DateTime.UtcNow;

                var result = await chatRepository.AddAsync(entity);

                return result > 0
                    ? new ServiceResponse(true, "Message sent successfully.")
                    : new ServiceResponse(false, "Failed to send message.");
            }
            catch (Exception ex)
            {
                return new ServiceResponse(false, $"Exception occurred while sending message: {ex.Message}");
            }
        }

        public async Task<List<ChatMessageDto>> GetPagedMessagesAsync(string? search, CancellationToken cancellationToken = default)
        {
            try
            {
                // Define includes if you want to include related entities, e.g., AppUser
                var includes = new Expression<Func<ChatMessage, object>>[] { };

                if (string.IsNullOrWhiteSpace(search))
                {
                    var allMessages = await chatRepository.GetPagedAsync(
                        search: null,
                        baseFilter: m => true,
                        searchProperties: null,
                        cancellationToken: cancellationToken,
                        includes: includes
                    );

                    return mapper.Map<List<ChatMessageDto>>(allMessages);
                }

                var searchProperties = new List<Expression<Func<ChatMessage, string>>>
                {
                    x => x.Message,
                    x => x.UserName,
                    x => x.UserId
                };

                var messages = await chatRepository.GetPagedAsync(
                    search,
                    m => true,
                    searchProperties,
                    cancellationToken,
                    includes
                );

                return mapper.Map<List<ChatMessageDto>>(messages);
            }
            catch
            {
                return new List<ChatMessageDto>();
            }
        }
    }
}

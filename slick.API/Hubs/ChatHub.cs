using Microsoft.AspNetCore.SignalR;
using slick.Application.DTOs.Chat;
using slick.Application.Services.Interfaces;

namespace slickAPI.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        // Client sends message
        public async Task SendMessage(CreateChatMessageDto messageDto)
        {
            // Save message to DB
            await _chatService.CreateMessageAsync(messageDto);

            // Broadcast to all connected clients
            await Clients.All.SendAsync("ReceiveMessage", messageDto);
        }

        // Client requests recent messages
        public async Task GetRecentMessages(int lastN = 50)
        {
            var messages = await _chatService.GetPagedMessagesAsync(null);
            await Clients.Caller.SendAsync("ReceiveRecentMessages", messages);
        }
    }
}

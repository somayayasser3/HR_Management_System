using HrManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HrManagementSystem.Repositories
{
    public class ChatRepository : GenericRepo<ChatMessage>
    {
        public ChatRepository(HRContext context) : base(context)
        {
        }

        public async Task<List<ChatMessage>> GetChatHistoryAsync()
        {
            return await con.ChatMessages
                .OrderBy(c => c.Timestamp)
                .Take(100) // Limit to last 100 messages
                .ToListAsync();
        }

        public async Task ClearChatHistoryAsync()
        {
            var messages = await con.ChatMessages.ToListAsync();
            con.ChatMessages.RemoveRange(messages);
            await con.SaveChangesAsync();
        }

        public async Task<List<ChatMessage>> GetRecentMessagesAsync(int count = 50)
        {
            return await con.ChatMessages
                .OrderByDescending(c => c.Timestamp)
                .Take(count)
                .OrderBy(c => c.Timestamp)
                .ToListAsync();
        }
        public async Task AddAsync(ChatMessage message)
        {
            await con.ChatMessages.AddAsync(message);
        }
    }
}

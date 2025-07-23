using HrManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HrManagementSystem.Repositories
{
    public class ChatRepository : GenericRepo<ChatMessage>
    {
        public ChatRepository(HRContext context) : base(context)
        {
        }

        public async Task AddAsync(ChatMessage message)
        {
            await con.ChatMessages.AddAsync(message);
        }
    }
}

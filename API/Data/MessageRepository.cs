using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages
                .OrderByDescending(x => x.MessageSent)
                .AsQueryable();

            // see which container user interested in seeing
            query = messageParams.Container switch
            {
                // get query for that particular container
                "Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username),
                "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username),
                _ => query.Where(u => u.RecipientUsername == messageParams.Username && u.DateRead == null)
            };
            // then project that to a MessageDto
            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);
            // which we then return as a PagedList
            return await PagedList<MessageDto>
                .CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var messages = await _context.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(
                    m => m.RecipientUsername == currentUsername &&
                    m.SenderUsername == recipientUsername ||
                    m.RecipientUsername == recipientUsername &&
                    m.SenderUsername == currentUsername
                )
                .OrderBy(m => m.MessageSent)
                .ToListAsync();
            // have the list in memory so don't need to go to database to get list of unread messages
            var unreadMessages = messages.Where(m => m.DateRead == null
                && m.RecipientUsername == currentUsername).ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    // mark as read as soon as received by recipient
                    message.DateRead = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }

            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}

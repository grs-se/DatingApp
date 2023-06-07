using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
// SignalR does not track who is in our group, for the same reason that it's not going
// to track is who is connected to SignalR, because we might have more than 1 SignalR
// server and it's got no way of interacting with its partners, 1,2,3,or 20 servers, 
// to know which of the users is in which of the groups. 
// This means we need to track who is in the group ourselves. 

// The optimal solution for this is to use Redis and to distribute Redis over
// multiple servers. 

// In this case we will just use the Database, which is an alternative approach to having
// a multi server solution for SignalR. We need to create an entity to do this.
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _presenceHub;

        public MessageHub(
            IMessageRepository messageRepository,
            IUserRepository userRepository,
            IMapper mapper,
            IHubContext<PresenceHub> presenceHub)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _presenceHub = presenceHub;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"];
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            // add user to group
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await _messageRepository
                .GetMessageThread(Context.User.GetUsername(), otherUser);
            // when user connects to this message hub, then they will receive 
            // messages from SignalR instead of from an API call as what was happening before.
            // We don't need to send the entire message thread down to the user that's 
            // already connected because they already have it.
            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        // when a user actually disconnects from SignalR, then they are automatically removed
        // from any groups they belong to.
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUsername();
            // Cannot return Http responses outside of a Controller so can't return BadRequest()
            // So throw an exception, but be specific with a HubException()
            // Exceptions are more expensive: they cost more resources on our server than a simple http resonse
            // but exceptions are exceptional.
            if (username == createMessageDto.RecipientUsername.ToLower())
            {
                throw new HubException("You cannot send messages to yourself");
            }

            var sender = await _userRepository.GetUserByUsernameAsync(username);
            var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) throw new HubException("Not found user");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            // If they are not connected they won't receive any notification
            // If they are inside the message group they get the message and we mark it as DateRead
            // Otherwise, if they are in any other part of our application and not connected to
            // the same message group as the user that is sending the message then 
            // we allow them to receive a notification that they've had a new message. 
            var group = await _messageRepository.GetMessageGroup(groupName);
            // check connections to see if we do have a username that matches
            // the recipient username, and if so we can mark the message as read
            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
                if (connections != null)
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                        // toast information
                        new { username = sender.UserName, knownAs = sender.KnownAs });
                }
            }

            _messageRepository.AddMessage(message);

            if (await _messageRepository.SaveAllAsync())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }
        }

        private string GetGroupName(string caller, string other)
        {
            // return a boolean with < 0
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";

        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await _messageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if (group == null)
            {
                group = new Group(groupName);
                _messageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if (await _messageRepository.SaveAllAsync()) return group;

            throw new HubException("Failed to add to group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            // Only removing from database, not from message group and SignalR, 
            // because when we do disconnect from SignalR, inside OnDisconnectedAsync, 
            // then SignalR will automatically remove them from the SignalR group
            // even though we don't have access to that information ourselves to query it.
            var group = await _messageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            _messageRepository.RemoveConnection(connection);

            if (await _messageRepository.SaveAllAsync()) return group;

            throw new HubException("Failed to remove from group");
        }
    }

    // if they are in the same group then we can mark them as read by the
    // recipient of the message.
}

﻿using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IMessageRepository _messageRepository;

        public MessageHub(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"];
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            // add user to group
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var messages = await _messageRepository
                .GetMessageThread(Context.User.GetUsername(), otherUser);
            // when user connects to this message hub, then they will receive 
            // messages from SignalR instead of from an API call as what was happening before.
            await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        private string GetGroupName(string caller, string other)
        {
            // return a boolean with < 0
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";

        }
    }
}
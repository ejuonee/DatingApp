using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.DTO;
using DatingApp.Entities;
using DatingApp.Extensions;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace DatingApp.SignalR
{
  public class MessageHub:Hub
  {
    // private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;
    private readonly IHubContext<PresenceHub> _presenceHub;
    private readonly PresenceTracker _presenceTracker;
    private readonly IUnitOfWork _unitOfWork;
    // private readonly IUserRepository _userRepository;
    public MessageHub(/* IMessageRepository messageRepository, */ IMapper mapper, /* IUserRepository userRepository, */ IHubContext<PresenceHub> presenceHub, PresenceTracker presenceTracker,IUnitOfWork unitOfWork)
    {
      _presenceTracker = presenceTracker;
      _mapper = mapper;
   /*    _messageRepository = messageRepository;
      _userRepository = userRepository; */
      _presenceHub = presenceHub;
      _unitOfWork = unitOfWork;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext.Request.Query["user"].ToString();
        var user = httpContext.User.Identity.Name;
        var users = Context.User.Identity.Name;
        var useee= Context.User.GetUserName();
        var groupName = user + "-" + otherUser;
        var groupNames = GetGroupName(useee,otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupNames);
        var group = await AddToGroup(groupName);

        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

        var messages = await _unitOfWork.MessageRepository.GetMessagesThread(useee, otherUser);

        if (_unitOfWork.HasChanges())
        {
            await _unitOfWork.Complete();
        }

        await Clients.Caller.SendAsync("ReceiveMessageThread", messages);

      await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {


        var group = await RemoveFromMessageGroup();
        await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
        await base.OnDisconnectedAsync(exception);
    //   return base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage (CreateMessageDto createMessageDto){
          var username = Context.User.GetUserName();
            
            if (username== createMessageDto.RecipientUsername.ToLower())
            {
                throw new HubException ("You cannot send a message to yourself");
            }
        

            // if (createMessageDto.RecipientId == createMessageDto.SenderId)
            // {
            //     return BadRequest("You cannot send a message to yourself");
            // }

            var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (sender == null)
            {
                throw new HubException("Sender not found");
            }

            if (recipient == null)
            {
                throw new HubException("Recipient not found");
            }
            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername= sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content,
            };

             var groupName = GetGroupName(sender.UserName,recipient.UserName);

            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);

            if (group.Connections.Any(x => x.Username == recipient.UserName)){
              message.DateRead = DateTime.UtcNow;
            }
            else{
              var connections= await _presenceTracker.GetConnectionsForUser(recipient.UserName);

              if (connections !=null){
                await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", new {
                  senderUsername = sender.UserName,
                  knownAs = sender.KnownAs,
                  recipientUsername = recipient.UserName,
                  content = createMessageDto.Content,
                  dateRead = DateTime.UtcNow
                });

                
              }
            }
             _unitOfWork.MessageRepository.AddMessage(message);

            
           
            if (await _unitOfWork.Complete())
            {
                

                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
                // return Ok(_mapper.Map<MessageDto>(message));
            }
                // return BadRequest("Message could not be sent");

    }

    private async Task<Group> AddToGroup(string groupName)
    {
        var group=  await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
        var connection = new Connection(Context.ConnectionId, Context.User.GetUserName());

        if (group == null)
        {
            group = new Group(groupName);
           
            _unitOfWork.MessageRepository.AddGroup(group);
             group.Connections.Add(connection);
        }
        else
        {
            group.Connections.Add(connection);
            // _messageRepository.UpdateMessageGroup(group);
        }

        if(await _unitOfWork.Complete())
        {
            return group;
        }
       
       throw new HubException("Could not add user to group");
        
    }

    private async Task<Group> RemoveFromMessageGroup(){
      var group = await _unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
      var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId); 
      _unitOfWork.MessageRepository.RemoveConnection(connection);

      if(await _unitOfWork.Complete()) return group;

      throw new HubException("Could not remove user from group");
    }
    private string GetGroupName(string caller, string other){
        var stringCompare = string.CompareOrdinal(caller,other)<0;

        return stringCompare?$"{caller}-{other}"  : $"{other}-{caller}";
    }
    
  }
    

  
}
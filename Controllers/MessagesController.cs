using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.DTO;
using DatingApp.Entities;
using DatingApp.Extensions;
using DatingApp.Helpers;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DatingApp.Controllers
{
    [Authorize]
    // [Route("[controller]")]
    public class MessagesController : BaseApiController
    {
        private readonly ILogger<MessagesController> _logger;
        // private readonly  IMessageRepository _messageRepository;
        // private readonly  IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IMapper _mapper;

        public MessagesController(
            IUnitOfWork unitOfWork,IMapper mapper,ILogger<MessagesController> logger)
            
            /* ILogger<MessagesController> logger, IMessageRepository messageRepository, IUserRepository userRepository, IMapper mapper */
        {
            _logger = logger;
            /* _messageRepository = messageRepository;
            _userRepository = userRepository; */
            _mapper = mapper; 
            _unitOfWork = unitOfWork;
        }

        [HttpPost]

        public async Task<ActionResult<MessageDto>> CreateMessage (CreateMessageDto createMessageDto)
        {

            var username = User.GetUserName();
            
            if (username== createMessageDto.RecipientUsername.ToLower())
            {
                return BadRequest("You cannot send a message to yourself");
            }
        

            // if (createMessageDto.RecipientId == createMessageDto.SenderId)
            // {
            //     return BadRequest("You cannot send a message to yourself");
            // }

            var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (sender == null)
            {
                return BadRequest("Sender not found");
            }

            if (recipient == null)
            {
                return BadRequest("Recipient not found");
            }
            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername= sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content,
            };
             _unitOfWork.MessageRepository.AddMessage(message);

            if (await _unitOfWork.Complete())
            {
                return Ok(_mapper.Map<MessageDto>(message));
            }
                return BadRequest("Message could not be sent");
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        
        {

            messageParams.Username = User.GetUserName();
            var messages = await _unitOfWork.MessageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

            return messages;


        }

        // [HttpGet("thread/{username}")]

        // public async Task <ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username){

        //     var currentUsername = User.GetUserName();

        //     return Ok(await _messageRepository.GetMessagesThread(currentUsername, username));

        // }
        //     // public IActionResult Index()
        // // {
        // //     return View();
        // // }

        // // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // // public IActionResult Error()
        // // {
        // //     return View("Error!");
        // // }

        [HttpDelete("{id}")]

        public async Task<ActionResult> DeleteMessage(int id)

        {
            var username = User.GetUserName();
            var message = await _unitOfWork.MessageRepository.GetMessage(id);
            if (message.Sender.UserName != username && message.Recipient.UserName != username)
            {
                return Unauthorized();
            }
            if (message.Sender.UserName == username)
            {
                message.SenderDeleted = true;
            }
            if (message.Recipient.UserName == username)
            {
                message.RecipientDeleted = true;
            }

            if (message.SenderDeleted && message.RecipientDeleted)
            {
                _unitOfWork.MessageRepository.Delete(message);
            }
            if (await _unitOfWork.Complete())
            {
                return NoContent();
            }

            return BadRequest("Message could not be deleted");
        }
    }
}
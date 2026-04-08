using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
//using Microsoft.Build.Tasks;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkExchange.Hubs;
using WorkExchange_API.DTO;
using WorkExchange_API.Models;
using WorkExchange_API.Services;



namespace WorkExchange_API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/chat")] //定義窗口網址路徑
    
    public class ChatApiController : ControllerBase
    {
        private readonly ChatService _chatService; //注入服務
        private readonly IHubContext<ChatHub> _hubContext;

        //透過建構函式注入
        public ChatApiController(ChatService chatService, IHubContext<ChatHub> hubContext)
        {
            _chatService = chatService;
            _hubContext = hubContext;
        }

        //取得當前使用者 ID 並轉為 int 的小工具(原本是字串)
        //GetCurrentUserIdInt()確認你是誰
        private int? GetCurrentUserIdInt()
        {
            var raw =
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        User.FindFirstValue("UserId");

            return int.TryParse(raw, out var id) ? id : null;
        }

        //取得房間清單API
        
        [HttpGet("rooms")]
        public async Task<IActionResult> GetRooms()
        {
            // 1. 先呼叫寫好的小工具，看看這張「識別證」是誰的
            var myId = GetCurrentUserIdInt();

            // 2. 如果 myId 是空的 (null)，代表這個人沒登入或識別證無效
            if (myId == null)
            {
                // 回傳 401 Unauthorized (你沒權限，請先登入)
                return Unauthorized("請先登入後再存取房間清單。");
            }

            // 3. 既然有 ID 了，就傳給 Service。
            // 注意：因為 myId 是 int? (可為空)，傳入時要寫 myId.Value 確保它是真正的數字
            var rooms = await _chatService.GetUserRooms(myId.Value);

            // 4. 最後把結果回傳給前端
            return Ok(rooms);
        }



        //rooms/messages API
        [HttpGet("rooms/{roomId}/messages")]
        public async Task<IActionResult> GetMessages(int roomId)
        {
            var myId = GetCurrentUserIdInt();
            if (myId == null) return Unauthorized();

            var isMember = await _chatService.IsRoomMember(roomId, myId.Value);

            if (!isMember)
            {
                return StatusCode(403, "你沒有權限存取此房間");
            }
            //抓取原始 Entity
            var entities = await _chatService.GetMessagesByRoomId(roomId);

            // 防呆: 如果 service 回傳 null，給他空列表
            if (entities == null) entities = new List<ChatMessage>();
            //獲取未讀數(在標記已讀前抓取)
            int unreadCount = await _chatService.GetUnreadCountForUser(roomId, myId.Value);

            int firstUnreadIndex = (unreadCount > 0 && unreadCount <= entities.Count)
                ? entities.Count - unreadCount
                : -1;

            var messages = entities.Select((m, index) => new ChatMessageDto
            {
                Id = m.Id,
                Content = m.Content ?? "",
                CreatedAt = m.CreatedAt ?? DateTime.Now,
                SenderUserId = m.SenderUserId ?? 0,
                IsFirstUnread = (index == firstUnreadIndex)
            }).ToList();
            return Ok(messages);
        }


        //規定用POST呼叫/ {roomId}變數-要傳到哪個房間
        [HttpPost("rooms/{roomId}/messages")]

        //int roomId-自動從網址抓出房間編號
        //[FromBody] MessageInput input-把前端傳來的 JSON（信件內容）自動塞進 input 這個物件裡
        public async Task<IActionResult> SendMessage(int roomId, [FromBody] MessageInput input)
        {

            // 檢查收到的內容是不是空的
            if (input == null || string.IsNullOrWhiteSpace(input.Content))
                return BadRequest("Content is required.");
            //在 SendMessage 裡面也需要取得當前使用者 ID
            var myId = GetCurrentUserIdInt();
            if (myId == null) return Unauthorized();

            var isMember = await _chatService.IsRoomMember(roomId, myId.Value);

            if (!isMember)
            {
                return StatusCode(403, "你沒有權限存取此房間");
            }

            // 存 DB
            var savedMsg = await _chatService.SaveMessage(roomId, myId.Value, input.Content);

            //即時廣播(SignalR)-主動推播核心邏輯
            //Group(roomId):只有「已經加入這個房間 ID」的人（通常是發送者和接收者）會收到通知
            await _hubContext.Clients
                    .Group(roomId.ToString())
                    .SendAsync("ReceiveMessage", savedMsg);

            return Ok(savedMsg);


        }
        // 點開聊天室，消除未讀紅點
        [HttpPost("rooms/{roomId}/read")]
        public async Task<IActionResult> MarkAsRead(int roomId)
        {
            // 1. 確認你是誰
            var myId = GetCurrentUserIdInt();
            if (myId == null) return Unauthorized();

            // 2. 呼叫 Service 執行「已讀」邏輯
            bool hasAnyUnread = await _chatService.MarkAsRead(roomId, myId.Value);

            return Ok(new { message = "已標記為已讀",
                hasAnyUnread = hasAnyUnread
            });
        }

        //檢查全域未讀
        [HttpGet("unread-status")]
        public async Task<IActionResult> GetUnreadStatus()
        {
            // 1. 確認你是誰
            var myId = GetCurrentUserIdInt();
            if (myId == null) return Unauthorized();

            // 2. 呼叫 Service 執行「已讀」邏輯
            bool hasAnyUnread = await _chatService.CheckAnyUnread( myId.Value);

            return Ok(new
            {
                message = "已標記為已讀",
                hasAnyUnread = hasAnyUnread
            });
            }

        public class MessageInput
        {
            public string Content { get; set; }
        }
    }

}

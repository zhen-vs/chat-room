using Microsoft.EntityFrameworkCore;
using WorkExchange_API.Models;

namespace WorkExchange_API.Services
{
    public class ChatService

    {
        //依賴注入-連接資料庫
        private readonly WorkExchangeDBContext _context;
        private readonly BlacklistService _blacklistService;

        public ChatService(WorkExchangeDBContext context, BlacklistService blacklistService)
        {
            _context = context;
            _blacklistService = blacklistService;
        }

        //取得房間邏輯
        public async Task<List<ChatRoomListDto>> GetUserRooms(int userId)
        {
            //先查我封鎖了誰
            var blockedIds = await _blacklistService.GetBlockedIdsAsync(userId); ;

            var rooms = await _context.ChatRoom
            .Where(r => r.UserId == userId || r.HostId == userId)
            .Select(r => new ChatRoomListDto
            {
                Id = r.Id,
                HostId = r.HostId,
                UserId = r.UserId,

                //  抓名字（各查一次 Users）
                HostName = _context.Users.Where(u => u.Id == r.HostId).Select(u => u.Name).FirstOrDefault() ?? "",

                HostCompanyName = _context.HostPage
                .Where(h => h.HostId == r.HostId)
                .Select(h => h.HostName)
                .FirstOrDefault() ?? "",

                UserName = _context.Users.Where(u => u.Id == r.UserId).Select(u => u.Name).FirstOrDefault() ?? "",

                //  最後一句（各房間查一次 ChatMessage）
                LastMessagePreview = _context.ChatMessage
                    .Where(m => m.RoomId == r.Id)
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => m.Content)
                    .FirstOrDefault(),

                LastMessageAt = _context.ChatMessage
                    .Where(m => m.RoomId == r.Id)
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => (DateTime?)m.CreatedAt)
                    .FirstOrDefault(),

                // 動態計數
                // 未讀條件：是這個房間的訊息、不是我發出的、且 IsRead 標記為 false
                Unread = _context.ChatMessage
                .Count(m => m.RoomId == r.Id && m.SenderUserId != userId && !m.IsRead),

                //封鎖狀態
                //目標對象 = 這個房間中「不是我」的那個人
                IsBlocked = ((r.UserId == userId ? r.HostId : r.UserId).HasValue) &&
        blockedIds.Contains((r.UserId == userId ? r.HostId : r.UserId)!.Value)
            })
            .OrderByDescending(x => x.LastMessageAt)
            .ToListAsync();

            return rooms;
        }

        // 取得特定房間的訊息紀錄
        public async Task<List<ChatMessage>> GetMessagesByRoomId(int roomId)
        {
            return await _context.ChatMessage
                .Where(m => m.RoomId == roomId)
                .OrderBy(m => m.CreatedAt) // 讓訊息按時間順序排列（舊到新）
                .ToListAsync();
        }

        //存 DB 的方法
        public async Task<ChatMessage> SaveMessage(int roomId, int senderUserId, string content)
        {
            var msg = new ChatMessage
            {
                RoomId = roomId,
                SenderUserId = senderUserId,
                Content = content,
                CreatedAt = DateTime.Now
            };

            _context.ChatMessage.Add(msg);
            await _context.SaveChangesAsync();

            return msg;
        }

        public async Task<bool> MarkAsRead(int roomId, int userId)
        {
            // 找出房間內不是我傳的且還沒讀的
            var unreadMessages = await _context.ChatMessage
        .Where(m => m.RoomId == roomId && m.SenderUserId != userId && !m.IsRead)
        .ToListAsync();

            if (unreadMessages.Any())
            {
                // 全部標記為已讀
                unreadMessages.ForEach(m => m.IsRead = true);
                await _context.SaveChangesAsync();
            }
            //已讀動作完成後，順便回傳：現在全站是否還有其他未讀？
            return await CheckAnyUnread(userId);
        }
        public async Task<bool> IsRoomMember(int roomId, int userId)
        {

            return await _context.ChatRoom.AnyAsync(r =>
            r.Id == roomId && (r.HostId == userId || r.UserId == userId));
        }

        public async Task<bool> CheckAnyUnread(int userId)
        {
            // 1. 先抓出我封鎖了誰
            var blockedIds = await _blacklistService.GetBlockedIdsAsync(userId);

            // 2. 使用 GetValueOrDefault() 來處理 int? 轉換問題
            return await _context.ChatMessage
                .AnyAsync(m => m.SenderUserId != userId &&
                               !m.IsRead &&
                               !blockedIds.Contains(m.SenderUserId.GetValueOrDefault()) &&
                               _context.ChatRoom.Any(r => r.Id == m.RoomId && (r.UserId == userId || r.HostId == userId)));
        }

        public async Task<int> GetUnreadCountForUser(int roomId, int userId)
        {
            return await _context.ChatMessage
                .CountAsync(m => m.RoomId == roomId && m.SenderUserId != userId && !m.IsRead);
        }
    }

}

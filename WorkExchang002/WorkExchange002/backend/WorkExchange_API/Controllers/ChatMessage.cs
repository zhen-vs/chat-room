using System.ComponentModel.DataAnnotations.Schema;

namespace WorkExchange.Models
{
    [Table("ChatMessage")] // 告訴 EF Core：對應資料庫的 ChatMessage 表（資料註記）
    public class ChatMessage
    {
        public int Id { get; set; }
        public int RoomId { get; set; } // 屬於哪個聊天室
        public string SenderId { get; set; } // 誰發的訊息
        public string Content { get; set; }  // 訊息內容
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false; // 支援前端未讀數顯示
    }
}

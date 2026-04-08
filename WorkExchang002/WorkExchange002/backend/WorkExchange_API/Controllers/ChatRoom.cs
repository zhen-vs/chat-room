using System.ComponentModel.DataAnnotations.Schema;

namespace WorkExchange.Models
{
    [Table("ChatRoom")] // 告訴 EF Core：資料庫裡這張表就叫 ChatRoom（資料註記）
    public class ChatRoom
    {
        public int Id { get; set; }
        public string UserId { get; set; } // 求職者
        public string HostId { get; set; } // 商家 
        public string? LastMessagePreview { get; set; } // 最後訊息預覽
        public DateTime CreatedAt { get; set; } = DateTime.Now; // 建立時間


        public DateTime? LastMessageAt { get; set; }
        public DateTime? UserLastReadAt { get; set; }
        public DateTime? HostLastReadAt { get; set; }

        // 關聯訊息
        public virtual ICollection<ChatMessage> ChatMessages { get; set; }
    }
}


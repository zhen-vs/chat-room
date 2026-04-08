namespace WorkExchange_API.DTO
{
    public class ChatMessageDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public int SenderUserId { get; set; }
        // 專門給前端畫線用的
        public bool IsFirstUnread { get; set; } = false;
    }
}

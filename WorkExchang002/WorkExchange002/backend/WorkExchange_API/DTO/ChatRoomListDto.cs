namespace WorkExchange_API.Models
{
    public class ChatRoomListDto
    {
        public int Id { get; set; }
        public int? HostId { get; set; }
        public int? UserId { get; set; }

        public string HostName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string? HostCompanyName { get; set; }

        public string? LastMessagePreview { get; set; }
        public DateTime? LastMessageAt { get; set; }

        public int Unread { get; set; }

        public bool IsBlocked { get; set; }
    }
}

namespace Decorations.Domain.Entities
{
    public class ContactMessage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime ReceivedAt { get; set; }
        public bool IsRead { get; set; }
    }
}

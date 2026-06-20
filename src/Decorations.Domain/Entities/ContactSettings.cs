namespace Decorations.Domain.Entities
{
    public class ContactSettings
    {
        public int Id { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string WhatsAppNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string InstagramUrl { get; set; } = string.Empty;
        public string FacebookUrl { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string BusinessHours { get; set; } = string.Empty;
    }
}

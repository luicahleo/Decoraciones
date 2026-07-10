namespace Decorations.Application.DTOs
{
    public class ContactSettingsDto
    {
        public int Id { get; set; }

        // El model binding convierte los campos vacíos del formulario en null,
        // por eso se declaran anulables; la capa de servicio los coalesce a
        // cadena vacía antes de persistir (columnas NOT NULL en la BD).
        public string? BusinessName { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? Email { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }
        public string? Address { get; set; }
        public string? BusinessHours { get; set; }
    }
}

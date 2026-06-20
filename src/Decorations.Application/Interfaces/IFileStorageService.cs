namespace Decorations.Application.Interfaces
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Guarda un archivo en la ruta especificada dentro de uploads
        /// </summary>
        /// <param name="content">Contenido del archivo en bytes</param>
        /// <param name="fileName">Nombre del archivo (ej: image.webp)</param>
        /// <param name="basePath">Ruta base relativa a uploads (ej: events/123/full-size)</param>
        /// <returns>Ruta relativa del archivo guardado (ej: /uploads/events/123/full-size/guid_image.webp)</returns>
        Task<string> SaveAsync(byte[] content, string fileName, string basePath = "");
        
        Task DeleteAsync(string relativePath);
    }
}

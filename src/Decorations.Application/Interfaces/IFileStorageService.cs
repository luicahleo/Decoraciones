namespace Decorations.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveAsync(byte[] content, string fileName);
        Task DeleteAsync(string relativePath);
    }
}

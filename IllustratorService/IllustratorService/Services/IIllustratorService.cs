

using IllustratorService.Models;

namespace IllustratorService.Services;

public interface IIllustratorService
{
    Task<IllustratorResponse> CreateAsync(CreateIllustratorRequest request, string userId);
    Task<IllustratorResponse?> GetByIdAsync(int id);
    Task<IllustratorResponse?> GetByUserIdAsync(string userId);
    Task<(IEnumerable<IllustratorResponse> Illustrators, int TotalCount)> GetAllAsync(
        int page = 1,
        int pageSize = 20,
        string? specialty = null,
        bool? isAvailableForWork = null);
    Task<IllustratorResponse> UpdateAsync(int id, UpdateIllustratorRequest request, string userId);
    Task DeleteAsync(int id, string userId);
}

using AutoMapper;
using slick.Application.DTOs;
using slick.Application.DTOs.Branch;
using slick.Application.Services.Interfaces;
using slick.Domain.Entities;
using slick.Domain.Interfaces;
using System.Linq.Expressions;

namespace slick.Application.Services.Implementations
{
    public class BranchService(IGeneric<Branch> branchRepository, IBackgroundJobService backgroundJobService,IMapper mapper) : IBranchService
    {
        public async Task<ServiceResponse> CreateBranchAsync(CreateBranchDto branchDto)
        {
            try
            {
                var entity = mapper.Map<Branch>(branchDto);
                entity.IsActive = true;
                entity.IsDeleted = false;
                var result = await branchRepository.AddAsync(entity);

                return result > 0
                    ? new ServiceResponse(true, " created successfully.")
                    : new ServiceResponse(false, "Failed to create Branch.");
            }
            catch
            {
                return new ServiceResponse(false, "Exception occurred while creating Branch.");
            }
        }
        public async Task<ServiceResponse> UpdateBranchAsync(UpdateBranchDto branchDto)
        {
            try
            {
                var existing = await branchRepository.GetByIdAsync(branchDto.ID);
                if (existing is null)
                    return new ServiceResponse(false, "Branch not found.");
                mapper.Map(branchDto, existing);
                existing.LastModifiedDate = DateTime.Now;

                var result = await branchRepository.UpdateAsync(existing);
                return result > 0
                    ? new ServiceResponse(true, "Branch updated successfully.")
                    : new ServiceResponse(false, "Failed to update Branch.");
            }
            catch
            {
                return new ServiceResponse(false, "Exception occurred while updating Branch.");
            }
        }
        public async Task<GetBranchDto?> GetBranchByIdAsync(Guid id)
        {
            try
            {
                var data = await branchRepository.GetByIdAsync(id);
                return data is null ? null : mapper.Map<GetBranchDto>(data);
            }
            catch
            {
                return null;
            }
        }
        public async Task<List<GetBranchDto>> GetAllBranchsAsync()
        {
            try
            {
                var carColors = await branchRepository.GetAllAsync(); ;
                return mapper.Map<List<GetBranchDto>>(carColors);
            }
            catch
            {
                return new List<GetBranchDto>();
            }
        }
        public async Task<List<GetBranchDto>> GetPagedBranchAsync(string? search)
        {
            try
            {
                // Define includes for navigation properties
                var includes = new Expression<Func<Branch, object>>[]
                {
                    x => x.Company
                };
                if (string.IsNullOrWhiteSpace(search))
                {
                    var allbranchess = await branchRepository.GetPagedAsync(
                        search: null,
                        baseFilter: x => x.IsActive && !x.IsDeleted,
                        searchProperties: null,
                        cancellationToken: default,
                        includes: includes
                    );
                    return mapper.Map<List<GetBranchDto>>(allbranchess);
                }

                var searchProperties = new List<Expression<Func<Branch, string>>>
                {
                    x => x.BranchName,
                    x => x.Company.Name,
                    x => x.BranchAddress!
                };
                var branches = await branchRepository.GetPagedAsync(
                  search,
                  x => x.IsActive && !x.IsDeleted,
                  searchProperties,
                  default,
                  includes
              );

                return mapper.Map<List<GetBranchDto>>(branches);
            }
            catch
            {
                return new List<GetBranchDto>();
            }
        }
        public async Task<ServiceResponse> DeleteBranchAsync(Guid id, string userid)
        {
            try
            {
                var result = await branchRepository.DeleteAsync(id, userid);
                return result > 0
                    ? new ServiceResponse(true, " Branch deleted successfully.")
                    : new ServiceResponse(false, " Branch not found or could not be deleted.");
            }
            catch
            {
                return new ServiceResponse(false, "Exception occurred while deleting Branch");
            }
        }
        public async Task<int> MultiSoftDeleteBranchAsync(List<Guid> ids, string userId, CancellationToken cancellationToken = default)
        {
            if (ids == null || !ids.Any())
            {
                return 0;
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            }

            if (!Guid.TryParse(userId, out Guid parsedUserId))
            {
                throw new ArgumentException("Invalid User ID format", nameof(userId));
            }

            try
            {
                return await branchRepository.MultiSoftDeleteAsync(
                    ids,
                    parsedUserId,
                    cancellationToken);
            }
            catch (InvalidOperationException ex) when (
                ex.Message.Contains("ID property") ||
                ex.Message.Contains("IsDeleted property"))
            {

                throw new InvalidOperationException("The Branch doesn't support soft delete operations", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to soft delete Branch", ex);
            }
        }
        public async Task<List<GetBranchDto>> GetTrashedBranchAsync(string? search)
        {
            try
            {
                Expression<Func<Branch, bool>> filter = x => true;

                if (!string.IsNullOrEmpty(search))
                {
                    string searchLower = search.ToLower();
                    filter = x =>
                        x.BranchName.ToLower().Contains(searchLower) ||
                        (x.BranchAddress != null && x.BranchAddress.ToLower().Contains(searchLower));
                }

                var branches = await branchRepository.GetTrashedAsync(filter);
                var branchDtos = mapper.Map<List<GetBranchDto>>(branches);

                // We'll store branches that should be deleted
                var toDelete = branchDtos.Where(b => b.DaysUntilHardDelete == 0).ToList();

                // Enqueue background jobs for them
                foreach (var branchDto in toDelete)
                {
                    backgroundJobService.Enqueue<IBranchService>(x => x.HardDeleteBranchsAsync(branchDto.ID));
                }

                // Remove them from the returned list
                branchDtos.RemoveAll(b => b.DaysUntilHardDelete == 0);

                return branchDtos;
            }
            catch
            {
                // Optional: log the exception for debugging
                return new List<GetBranchDto>();
            }
        }
        public async Task<bool> RecoverBranchAsync(Guid id, CancellationToken cancellationToken = default)
        {
            int result = await branchRepository.RecoverAsync(id, cancellationToken);
            return result > 0;
        }
        public async Task<int> MultiRecoverBranchsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
        {
            if (ids == null || !ids.Any()) return 0;
            return await branchRepository.MultiRecoverAsync(ids, cancellationToken);
        }
        public async Task<ServiceResponse> HardDeleteBranchsAsync(Guid id)
        {
            try
            {
                var result = await branchRepository.HardDeletewithcheckAsync(id);
                return result > 0
                    ? new ServiceResponse(true, "Branch permanently deleted successfully.")
                    : new ServiceResponse(false, "Branch not found or could not be permanently deleted.");
            }
            catch (InvalidOperationException ex)
            {
                // This will catch the "referenced by other records" exception
                return new ServiceResponse(false, ex.Message);
            }
            catch (Exception ex)
            {
                // Log the full exception for debugging, but show user-friendly message
                // You might want to log ex.ToString() here
                return new ServiceResponse(false, $"An error occurred while deleting the branch: {ex.Message}");
            }
        }
        public async Task<int> MultiHardDeleteBranchsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
        {
            return await branchRepository.MultiHardDeleteAsync(ids, cancellationToken);
        }
        public async Task<int> GetDeletedBranchsCountAsync()
        {
            return await branchRepository.CountDeletedAsync();
        }
        public async Task<IEnumerable<GetBranchNameDto>> GetBranchNameAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Get both ID and BusinessGroupName
                var results = await branchRepository.GetPropertyValuesAsync<GetBranchNameDto>(
                    selectExpression: e => new GetBranchNameDto
                    {
                        ID = e.ID,
                        BranchName = e.BranchName
                    },
                    cancellationToken: cancellationToken);

                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<GetBranchDto>> GetBranchesByCompanyIdAsync(Guid companyId)
        {
            // Use the existing generic method in your repository
            var branches = await branchRepository.GetByRelationalIdAsync(
                foreignKeyPropertyName: "CompanyId",
                foreignKeyValue: companyId,
                includes: x => x.Company
            );

            return mapper.Map<List<GetBranchDto>>(branches);
        }


    }
}

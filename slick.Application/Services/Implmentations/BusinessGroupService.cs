using AutoMapper;
using slick.Application.DTOs;
using slick.Application.DTOs.Branch;
using slick.Application.DTOs.BusinessGroup;
using slick.Application.Services.Interfaces;
using slick.Domain.Entities;
using slick.Domain.Interfaces;
using System.Linq.Expressions;
using System.Reflection;

namespace slick.Application.Services.Implementations
{
    public class BusinessGroupService(IGeneric<BusinessGroup> businessGroupRepository,IBackgroundJobService backgroundJobService, IMapper mapper) : IBusinessGroupService
    {
        public async Task<ServiceResponse> CreateBusinessGroupAsync(CreateBusinessGroupDto businessGroupDto)
        {
            try
            {
                var entity = mapper.Map<BusinessGroup>(businessGroupDto);
                entity.IsActive = true;
                entity.IsDeleted = false;
                var result = await businessGroupRepository.AddAsync(entity);

                return result > 0
                    ? new ServiceResponse(true, " created successfully.")
                    : new ServiceResponse(false, "Failed to create Business Group.");
            }
            catch
            {
                return new ServiceResponse(false, "Exception occurred while creating Business Group.");
            }
        }
        public async Task<ServiceResponse> UpdateBusinessGroupAsync(UpdateBusinessGroupDto businessGroupDto)
        {
            try
            {
                var existing = await businessGroupRepository.GetByIdAsync(businessGroupDto.ID);
                if (existing is null)
                    return new ServiceResponse(false, "Branch not found.");

                mapper.Map(businessGroupDto, existing);
                existing.LastModifiedDate = DateTime.Now;

                var result = await businessGroupRepository.UpdateAsync(existing);
                return result > 0
                    ? new ServiceResponse(true, "Business Group updated successfully.")
                    : new ServiceResponse(false, "Failed to update Business Group.");
            }
            catch
            {
                return new ServiceResponse(false, "Exception occurred while updating Business Group.");
            }
        }
        public async Task<GetBusinessGroupDto?> GetBusinessGroupByIdAsync(Guid id)
        {
            try
            {
                var data = await businessGroupRepository.GetByIdAsync(id);
                return data is null ? null : mapper.Map<GetBusinessGroupDto>(data);
            }
            catch
            {
                return null;
            }
        }
        public async Task<List<GetBusinessGroupDto>> GetAllBusinesGroupsAsync()
        {
            try
            {
                var businessGroups = await businessGroupRepository.GetAllAsync(); ;
                return mapper.Map<List<GetBusinessGroupDto>>(businessGroups);
            }
            catch
            {
                return new List<GetBusinessGroupDto>();
            }
        }
        public async Task<List<GetBusinessGroupDto>> GetPagedBusinessGroupAsync(string? search)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(search))
                {
                    var allBusinessGroup = await businessGroupRepository.GetAllAsync();
                    return mapper.Map<List<GetBusinessGroupDto>>(allBusinessGroup);
                }
                var searchProperties = new List<Expression<Func<BusinessGroup, string>>>
        {
            x => x.BusinessGroupName,
            x => x.BusinessGroupDescription!
        };

                var businessGroups = await businessGroupRepository.GetPagedAsync(
                    search,
                    x => x.IsActive && !x.IsDeleted,
                    searchProperties
                );

                return mapper.Map<List<GetBusinessGroupDto>>(businessGroups);
            }
            catch
            {
                return new List<GetBusinessGroupDto>();
            }
        }
        public async Task<ServiceResponse> DeleteBusinessGroupAsync(Guid id, string userid)
        {
            try
            {
                var result = await businessGroupRepository.DeleteAsync(id, userid);
                return result > 0
                    ? new ServiceResponse(true, " Business Group deleted successfully.")
                    : new ServiceResponse(false, " Business Group not found or could not be deleted.");
            }
            catch
            {
                return new ServiceResponse(false, "Exception occurred while deleting  Business Group");
            }
        }
        public async Task<int> MultiSoftDeleteBusinessGroupAsync(List<Guid> ids, string userId, CancellationToken cancellationToken = default)
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
                return await businessGroupRepository.MultiSoftDeleteAsync(
                    ids,
                    parsedUserId,
                    cancellationToken);
            }
            catch (InvalidOperationException ex) when (
                ex.Message.Contains("ID property") ||
                ex.Message.Contains("IsDeleted property"))
            {

                throw new InvalidOperationException("The Business Group doesn't support soft delete operations", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to soft delete Business Group", ex);
            }
        }
        public async Task<List<GetBusinessGroupDto>> GetTrashedBusinessGroupAsync(string? search)
        {
            try
            {
                Expression<Func<BusinessGroup, bool>> filter = x => true;

                if (!string.IsNullOrEmpty(search))
                {
                    string searchLower = search.ToLower();
                    filter = x =>
                        x.BusinessGroupName.ToLower().Contains(searchLower) ||
                         (x.BusinessGroupDescription != null && x.BusinessGroupDescription.ToLower().Contains(searchLower));
                }

                var businessGroups = await businessGroupRepository.GetTrashedAsync(filter);
                var businessGroupDTOs = mapper.Map<List<GetBusinessGroupDto>>(businessGroups);

                // We'll store branches that should be deleted
                var toDelete = businessGroupDTOs.Where(b => b.DaysUntilHardDelete == 0).ToList();

                // Enqueue background jobs for them
                foreach (var bussinessGroupDto in toDelete)
                {
                    backgroundJobService.Enqueue<IBusinessGroupService>(x => x.HardDeleteBusinessGroupsAsync(bussinessGroupDto.Id));
                }

                // Remove them from the returned list
                businessGroupDTOs.RemoveAll(b => b.DaysUntilHardDelete == 0);

                return businessGroupDTOs;
            }
            catch
            {
                return new List<GetBusinessGroupDto>();
            }
        }
        public async Task<bool> RecoverBusinessGroupAsync(Guid id, CancellationToken cancellationToken = default)
        {
            int result = await businessGroupRepository.RecoverAsync(id, cancellationToken);
            return result > 0;
        }
        public async Task<int> MultiRecoverBusinessGroupsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
        {
            if (ids == null || !ids.Any()) return 0;
            return await businessGroupRepository.MultiRecoverAsync(ids, cancellationToken);
        }
        public async Task<ServiceResponse> HardDeleteBusinessGroupsAsync(Guid id)
        {
            try
            {
                var result = await businessGroupRepository.HardDeleteAsync(id);
                return result > 0
                    ? new ServiceResponse(true, "  Business Group permanently deleted successfully.")
                    : new ServiceResponse(false, "  Business Group not found or could not be permanently deleted.");
            }
            catch
            {
                return new ServiceResponse(false, "Exception occurred while permanently deleting Business Group.");
            }
        }
        public async Task<int> MultiHardDeleteBusinessGroupsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
        {
            return await businessGroupRepository.MultiHardDeleteAsync(ids, cancellationToken);
        }
        public async Task<int> GetDeletedBusinessGroupsCountAsync()
        {
            return await businessGroupRepository.CountDeletedAsync();
        }
        public async Task<IEnumerable<GetBusinessGroupNameDto>> GetBusinessGroupNameAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Get both ID and BusinessGroupName
                var results = await businessGroupRepository.GetPropertyValuesAsync<GetBusinessGroupNameDto>(
                    selectExpression: e => new GetBusinessGroupNameDto
                    {
                        ID = e.ID,
                        BusinessGroupName = e.BusinessGroupName
                    },
                    cancellationToken: cancellationToken);

                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }



    }
}

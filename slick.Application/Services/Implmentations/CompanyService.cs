using AutoMapper;
using slick.Application.DTOs;
using slick.Application.DTOs.Campus;
using slick.Application.DTOs.Company;
using slick.Application.DTOs.CompanyAccount;
using slick.Application.Services.Interfaces;
using slick.Domain.Entities;
using slick.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;
using System.Reflection;

namespace slick.Application.Services.Implementations
{
    public class CompanyService(IGeneric<Company> companyRepository,IFileStorageService fileStorageService,IBackgroundJobService backgroundJobService,IConfiguration configuration   ,IMapper mapper) : ICompanyService
    {
        public async Task<ServiceResponse> CreateCompanyAsync(CreateCompanyDto companyDto)
        {
            try
            {
                if (companyDto == null)
                    return new ServiceResponse(false, "Company data is null.");

                var entity = mapper.Map<Company>(companyDto);
                entity.IsActive = true;
                entity.IsDeleted = false;

                if (companyDto.Logo != null && companyDto.Logo.Length > 0)
                {
                    var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp" };
                    var extension = Path.GetExtension(companyDto.Logo.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                        return new ServiceResponse(false, "Invalid file type. Only PNG, JPG, JPEG, GIF, and WEBP are allowed.");

                    if (companyDto.Logo.Length > 5 * 1024 * 1024)
                        return new ServiceResponse(false, "File size exceeds 5MB limit.");

                    // ✅ Save directly under StoragePath (no subfolder)
                    var storagePath = configuration["FileStorageSettings:StoragePath"] ?? "C://AppData";
                    var savedPath = await fileStorageService.SaveFileAsync(companyDto.Logo, "");

                    // 🔥 Remove "/Files/" prefix
                    if (!string.IsNullOrEmpty(savedPath) && savedPath.StartsWith("/Files/"))
                    {
                        savedPath = savedPath.Replace("/Files/", "");
                    }

                    entity.Logo = savedPath;
                }

                var result = await companyRepository.AddAsync(entity);

                return result > 0
                    ? new ServiceResponse(true, "Company created successfully.")
                    : new ServiceResponse(false, "Failed to create company.");
            }
            catch (Exception ex)
            {
                return new ServiceResponse(false, $"Exception occurred while creating company: {ex.Message}");
            }
        }
        public async Task<ServiceResponse> UpdateCompanyAsync(UpdateCompanyDto companyDto)
        {
            try
            {
                if (companyDto == null)
                    return new ServiceResponse(false, "Company data is null.");

                var existing = await companyRepository.GetByIdAsync(companyDto.ID);
                if (existing == null)
                    return new ServiceResponse(false, "Company not found.");

                var storagePath = configuration["FileStorageSettings:StoragePath"] ?? "C://AppData";

                if (companyDto.RemoveLogo && !string.IsNullOrEmpty(existing.Logo))
                {
                    fileStorageService.DeleteFile(existing.Logo, "");
                    existing.Logo = null;
                }

                if (companyDto.Logo != null && companyDto.Logo.Length > 0)
                {
                    var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp" };
                    var extension = Path.GetExtension(companyDto.Logo.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                        return new ServiceResponse(false, "Invalid file type. Only PNG, JPG, JPEG, GIF, and WEBP are allowed.");

                    if (companyDto.Logo.Length > 5 * 1024 * 1024)
                        return new ServiceResponse(false, "File size exceeds 5MB limit.");

                    if (!string.IsNullOrEmpty(existing.Logo))
                    {
                        fileStorageService.DeleteFile(existing.Logo, "");
                    }

                    // ✅ Save directly under StoragePath (no subfolder)
                    var savedPath = await fileStorageService.SaveFileAsync(companyDto.Logo, "");

                    // 🔥 Remove "/Files/" prefix
                    if (!string.IsNullOrEmpty(savedPath) && savedPath.StartsWith("/Files/"))
                    {
                        savedPath = savedPath.Replace("/Files/", "");
                    }

                    existing.Logo = savedPath;
                }

                mapper.Map(companyDto, existing);
                existing.LastModifiedDate = DateTime.Now;

                var result = await companyRepository.UpdateAsync(existing);

                return result > 0
                    ? new ServiceResponse(true, "Company updated successfully.")
                    : new ServiceResponse(false, "Failed to update company.");
            }
            catch (Exception ex)
            {
                return new ServiceResponse(false, $"Exception occurred while updating company: {ex.Message}");
            }
        }
        public async Task<GetCompanyDto?> GetCompanyByIdAsync(Guid id)
        {
            try
            {
                var data = await companyRepository.GetByIdAsync(id);
                return data is null ? null : mapper.Map<GetCompanyDto>(data);
            }
            catch
            {
                return null;
            }
        }
        public async Task<List<GetCompanyDto>> GetAllCompanyAsync()
        {
            try
            {
                var companies = await companyRepository.GetAllAsync(); ;
                return mapper.Map<List<GetCompanyDto>>(companies);
            }
            catch
            {
                return new List<GetCompanyDto>();
            }
        }
        public async Task<List<GetCompanyDto>> GetPagedCompanysAsync(string? search)
        {
            try
            {
                // Define includes for navigation properties
                var includes = new Expression<Func<Company, object>>[]
                {
            x => x.BusinessGroup,
            x => x.Branches
                };

                if (string.IsNullOrWhiteSpace(search))
                {
                    var allCompanies = await companyRepository.GetPagedAsync(
                        search: null,
                        baseFilter: x => x.IsActive && !x.IsDeleted,
                        searchProperties: null,
                        cancellationToken: default,
                        includes: includes
                    );
                    return mapper.Map<List<GetCompanyDto>>(allCompanies);
                }

                var searchProperties = new List<Expression<Func<Company, string>>>
        {
            x => x.Name,
            x => x.BusinessGroup.BusinessGroupName,
            x => x.Address,
            x => x.Email,
            x => x.OfficePhone!,
            x => x.TinNo!
        };

                var companies = await companyRepository.GetPagedAsync(
                    search,
                    x => x.IsActive && !x.IsDeleted,
                    searchProperties,
                    default,
                    includes
                );

                return mapper.Map<List<GetCompanyDto>>(companies);
            }
            catch
            {
                return new List<GetCompanyDto>();
            }
        }
        public async Task<ServiceResponse> DeleteCompanyAsync(Guid id, string userid)
        {
            try
            {
                var result = await companyRepository.DeleteAsync(id, userid);
                return result > 0
                    ? new ServiceResponse(true, "Company deleted successfully.")
                    : new ServiceResponse(false, "Company not found or could not be deleted.");
            }
            catch
            {
                return new ServiceResponse(false, "Exception occurred while deleting Company.");
            }
        }
        public async Task<int> MultiSoftDeleteCompanysAsync(List<Guid> ids, string userId, CancellationToken cancellationToken = default)
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
                return await companyRepository.MultiSoftDeleteAsync(
                    ids,
                    parsedUserId,
                    cancellationToken);
            }
            catch (InvalidOperationException ex) when (
                ex.Message.Contains("ID property") ||
                ex.Message.Contains("IsDeleted property"))
            {
                throw new InvalidOperationException("The Company entity doesn't support soft delete operations", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to soft delete companies", ex);
            }
        }
        public async Task<List<GetCompanyDto>> GetTrashedCompanysAsync(string? search)
        {
            try
            {
                Expression<Func<Company, bool>> filter = x => true;

                if (!string.IsNullOrEmpty(search))
                {
                    string searchLower = search.ToLower();
                    filter = x =>
                        x.Name.ToLower().Contains(searchLower) ||
                        x.TinNo.ToLower().Contains(searchLower) ||
                         x.BusinessGroup.BusinessGroupName.ToLower().Contains(searchLower) ||
                        (x.OfficePhone!= null && x.OfficePhone.ToLower().Contains(searchLower));
                }

                var companies = await companyRepository.GetTrashedAsync(filter);
                var companiesDTOs = mapper.Map<List<GetCompanyDto>>(companies);

                // We'll store branches that should be deleted
                var toDelete = companiesDTOs.Where(b => b.DaysUntilHardDelete == 0).ToList();

                // Enqueue background jobs for them
                foreach (var companiesDTO in toDelete)
                {
                    backgroundJobService.Enqueue<ICompanyService>(x => x.HardDeleteCompanyAsync(companiesDTO.ID));
                }

                // Remove them from the returned list
                companiesDTOs.RemoveAll(b => b.DaysUntilHardDelete == 0);

                return companiesDTOs;
            }
            catch
            {
                return new List<GetCompanyDto>();
            }
        }
        public async Task<bool> RecoverCompanyAsync(Guid id, CancellationToken cancellationToken = default)
        {
            int result = await companyRepository.RecoverAsync(id, cancellationToken);
            return result > 0;
        }
        public async Task<int> MultiRecoverCompanysAsync(List<Guid> ids, CancellationToken cancellationToken = default)
        {
            if (ids == null || !ids.Any()) return 0;
            return await companyRepository.MultiRecoverAsync(ids, cancellationToken);
        }
        public async Task<ServiceResponse> HardDeleteCompanyAsync(Guid id)
        {
            try
            {
                var result = await companyRepository.HardDeleteAsync(id);
                return result > 0
                    ? new ServiceResponse(true, "Company permanently deleted successfully.")
                    : new ServiceResponse(false, "Company not found or could not be permanently deleted.");
            }
            catch
            {
                return new ServiceResponse(false, "Exception occurred while permanently deleting Company.");
            }
        }
        public async Task<int> MultiHardDeleteCompanysAsync(List<Guid> ids, CancellationToken cancellationToken = default)
        {
            return await companyRepository.MultiHardDeleteAsync(ids, cancellationToken);
        }
        public async Task<int> GetDeletedCompanysCountAsync()
        {
            return await companyRepository.CountDeletedAsync();
        }
        public async Task<IEnumerable<GetCompanyNameDto>> GetCompanyNamesAsync(CancellationToken cancellationToken = default)
        {
            try
            {

                var results = await companyRepository.GetPropertyValuesAsync<GetCompanyNameDto>(
                    selectExpression: e => new GetCompanyNameDto
                    {
                        ID = e.ID,
                        Name = e.Name,
                        Address= e.Address,
                        Website=e.Website,
                        Email=e.Email,
                        MobileNo=e.MobileNo,
                        OfficePhone=e.OfficePhone,
                        TinNo=e.TinNo,
                        Logo=e.Logo

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

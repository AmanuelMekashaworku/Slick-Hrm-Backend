using AutoMapper;
using slick.Application.DTOs;
using slick.Application.DTOs.UserLog;
using slick.Application.Services.Interfaces;
using slick.Domain.Entities;
using slick.Domain.Interfaces;
using System.Linq.Expressions;

namespace slick.Application.Services.Implementations
{
    public class UserActivityLogService(IGeneric<UserActivityLog> logRepository,IMapper mapper) : IUserActivityLogService
    {
    
            public async Task<ServiceResponse> CreateLogAsync(CreateUserActivityLogDto logDto)
            {
                try
                {
                    if (logDto == null)
                        return new ServiceResponse(false, "Log data is null.");

                    var entity = mapper.Map<UserActivityLog>(logDto);
                    entity.CreatedDate = DateTime.UtcNow;

                    var result = await logRepository.AddAsync(entity);

                    return result > 0
                        ? new ServiceResponse(true, "Activity logged successfully.")
                        : new ServiceResponse(false, "Failed to log activity.");
                }
                catch (Exception ex)
                {
                    return new ServiceResponse(false, $"Exception occurred while logging activity: {ex.Message}");
                }
            }
        public async Task<List<UserActivityLogDto>> GetPagedLogsAsync(string? search, CancellationToken cancellationToken = default)
        {
            try
            {
               
                var includes = new Expression<Func<UserActivityLog, object>>[]
                {
                     x => x.AppUser 
                };

                if (string.IsNullOrWhiteSpace(search))
                {
                    var allLogs = await logRepository.GetPagedAsync(
                        search: null,
                        baseFilter: l => true,
                        searchProperties: null,
                        cancellationToken: cancellationToken,
                        includes: includes
                    );

                    return mapper.Map<List<UserActivityLogDto>>(allLogs);
                }

                var searchProperties = new List<Expression<Func<UserActivityLog, string>>>
                    {
                        x => x.Action,
                        x => x.Description!,
                        x => x.UserName!,
                        x => x.AppUser!.FirstName! 
                    };

                var logs = await logRepository.GetPagedAsync(
                    search,
                    l => true,
                    searchProperties,
                    cancellationToken,
                    includes
                );

                return mapper.Map<List<UserActivityLogDto>>(logs);
            }
            catch
            {
                return new List<UserActivityLogDto>();
            }
        }

    }
}

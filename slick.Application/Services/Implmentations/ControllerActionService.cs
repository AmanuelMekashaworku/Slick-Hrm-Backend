using AutoMapper;
using slick.Application.DTOs;
using slick.Application.DTOs.Branch;
using slick.Application.DTOs.ControllerAction;
using slick.Application.Services.Interfaces;
using slick.Domain.Entities;
using slick.Domain.Interfaces;
using SMS.Domain.Models;
using System.Linq.Expressions;

namespace slick.Application.Services.Implementations
{
    public class ControllerActionService(IGeneric<ControllerAction> controllerActionRepository, IMapper mapper) : IControllerActionService
    {
      
        public async Task<GetControllerActionDto?> GetControllerActionByIdAsync(Guid id)
        {
            try
            {
                var data = await controllerActionRepository.GetByIdAsync(id);
                return data is null ? null : mapper.Map<GetControllerActionDto>(data);
            }
            catch
            {
                return null;
            }
        }
        public async Task<List<GetControllerActionDto>> GetPagedControllerActionAsync(string? search)
        {
            try
            {
                // Define includes for navigation properties
                var includes = new Expression<Func<ControllerAction, object>>[]
                {
                    x => x.TaskController,
                    x => x.ActionTask,
                };
                if (string.IsNullOrWhiteSpace(search))
                {
                    var allcontrollerActions = await controllerActionRepository.GetPagedAsync(
                        search: null,
                        baseFilter: x => x.IsActive && !x.IsDeleted,
                        searchProperties: null,
                        cancellationToken: default,
                        includes: includes
                    );
                    return mapper.Map<List<GetControllerActionDto>>(allcontrollerActions);
                }

                var searchProperties = new List<Expression<Func<ControllerAction, string>>>
                {
                    x => x.ActionTask.ActionName,
                    x => x.TaskController.DisplayController,
                };
                var controllerActions = await controllerActionRepository.GetPagedAsync(
                  search,
                  x => x.IsActive && !x.IsDeleted,
                  searchProperties,
                  default,
                  includes
              );

                return mapper.Map<List<GetControllerActionDto>>(controllerActions);
            }
            catch
            {
                return new List<GetControllerActionDto>();
            }
        }

    }
}

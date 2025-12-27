using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.DTOs.User;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Entities;
using AspireApp.ApiService.Domain.Interfaces;

namespace AspireApp.ApiService.Application.UseCases.Users;

public class AssignRoleToUserUseCase : BaseUseCase
{
    private readonly IUserRepository _userRepository;

    public AssignRoleToUserUseCase(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> ExecuteAsync(Guid userId, AssignRoleToUserRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            // Verify user exists
            var userExists = await _userRepository.ExistsAsync(userId, ct);
            if (!userExists)
            {
                return Result.Failure(DomainErrors.User.NotFound(userId));
            }

            // Use generic repository to manage UserRole entities directly via DbSet
            // This bypasses EF Core's problematic backing field change tracking
            var userRoleRepo = UnitOfWork.GetRepository<UserRole>();
            var roleIdSet = request.RoleIds.ToHashSet();

            // Get ALL existing UserRoles for this user (including soft-deleted)
            var existingUserRoles = await userRoleRepo.GetListAsync(
                ur => ur.UserId == userId, 
                includeDeleted: true, 
                ct);

            // Soft-delete active roles that should be removed
            foreach (var userRole in existingUserRoles.Where(ur => !ur.IsDeleted && !roleIdSet.Contains(ur.RoleId)))
            {
                await userRoleRepo.DeleteAsync(userRole, ct);
            }

            // Restore soft-deleted roles that should be re-added
            foreach (var userRole in existingUserRoles.Where(ur => ur.IsDeleted && roleIdSet.Contains(ur.RoleId)))
            {
                userRole.Restore();
            }

            // Add only truly new roles (never existed before)
            var allExistingRoleIds = existingUserRoles.Select(ur => ur.RoleId).ToHashSet();
            foreach (var roleId in roleIdSet.Where(id => !allExistingRoleIds.Contains(id)))
            {
                await userRoleRepo.InsertAsync(new UserRole(userId, roleId), ct);
            }

            return Result.Success();
        }, cancellationToken);
    }
}

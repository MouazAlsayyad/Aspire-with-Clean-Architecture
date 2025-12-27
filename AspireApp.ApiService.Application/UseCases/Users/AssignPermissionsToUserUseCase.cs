using AspireApp.ApiService.Application.Common;
using AspireApp.ApiService.Application.DTOs.User;
using AspireApp.ApiService.Domain.Common;
using AspireApp.ApiService.Domain.Entities;
using AspireApp.ApiService.Domain.Interfaces;

namespace AspireApp.ApiService.Application.UseCases.Users;

public class AssignPermissionsToUserUseCase : BaseUseCase
{
    private readonly IUserRepository _userRepository;

    public AssignPermissionsToUserUseCase(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper)
        : base(unitOfWork, mapper)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> ExecuteAsync(Guid userId, AssignPermissionsToUserRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteAndSaveAsync(async ct =>
        {
            // Verify user exists
            var userExists = await _userRepository.ExistsAsync(userId, ct);
            if (!userExists)
            {
                return Result.Failure(DomainErrors.User.NotFound(userId));
            }

            // Use generic repository to manage UserPermission entities directly via DbSet
            // This bypasses EF Core's problematic backing field change tracking
            var userPermissionRepo = UnitOfWork.GetRepository<UserPermission>();
            var permissionIdSet = request.PermissionIds.ToHashSet();

            // Get ALL existing UserPermissions for this user (including soft-deleted)
            var existingUserPermissions = await userPermissionRepo.GetListAsync(
                up => up.UserId == userId,
                includeDeleted: true,
                ct);

            // Soft-delete active permissions that should be removed
            foreach (var userPermission in existingUserPermissions.Where(up => !up.IsDeleted && !permissionIdSet.Contains(up.PermissionId)))
            {
                await userPermissionRepo.DeleteAsync(userPermission, ct);
            }

            // Restore soft-deleted permissions that should be re-added
            foreach (var userPermission in existingUserPermissions.Where(up => up.IsDeleted && permissionIdSet.Contains(up.PermissionId)))
            {
                userPermission.Restore();
            }

            // Add only truly new permissions (never existed before)
            var allExistingPermissionIds = existingUserPermissions.Select(up => up.PermissionId).ToHashSet();
            foreach (var permissionId in permissionIdSet.Where(id => !allExistingPermissionIds.Contains(id)))
            {
                await userPermissionRepo.InsertAsync(new UserPermission(userId, permissionId), ct);
            }

            return Result.Success();
        }, cancellationToken);
    }
}


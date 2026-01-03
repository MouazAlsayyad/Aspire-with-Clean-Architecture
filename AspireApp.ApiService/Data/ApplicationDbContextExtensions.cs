using AspireApp.ApiService.Domain.ActivityLogs.Entities;
using AspireApp.ApiService.Infrastructure.Data;
using AspireApp.Modules.FileUpload.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.ApiService.Data;

/// <summary>
/// Extensions to ApplicationDbContext for module entities
/// This file is in the ApiService project to avoid circular dependencies
/// </summary>
public static class ApplicationDbContextExtensions
{
    public static DbSet<ActivityLog> ActivityLogs(this ApplicationDbContext context) => context.Set<ActivityLog>();
    public static DbSet<FileUpload> FileUploads(this ApplicationDbContext context) => context.Set<FileUpload>();
}


using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Entities;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Repositories;

namespace RouteGuard.Platform.StakeholderAssetManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class StudentGroupRepository(AppDbContext context)
    : BaseRepository<StudentGroup>(context), IStudentGroupRepository
{
    public Task<StudentGroup?> FindByStudentGroupIdAsync(StudentGroupId studentGroupId,
        CancellationToken cancellationToken) =>
        Context.Set<StudentGroup>().FirstOrDefaultAsync(group => group.Id == studentGroupId, cancellationToken);
}

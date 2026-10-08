using RouteGuard.Platform.Shared.Domain.Repositories;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Entities;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.ValueObjects;

namespace RouteGuard.Platform.StakeholderAssetManagement.Domain.Repositories;

public interface IStudentGroupRepository : IBaseRepository<StudentGroup>
{
    Task<StudentGroup?> FindByStudentGroupIdAsync(StudentGroupId studentGroupId, CancellationToken cancellationToken);
}

using RouteGuard.Platform.Shared.Domain.Repositories;
using RouteGuard.Platform.Stakeholder.Domain.Model.Entities;
using RouteGuard.Platform.Stakeholder.Domain.Model.ValueObjects;

namespace RouteGuard.Platform.Stakeholder.Domain.Repositories;

public interface IStudentGroupRepository : IBaseRepository<StudentGroup>
{
    Task<StudentGroup?> FindByStudentGroupIdAsync(StudentGroupId studentGroupId, CancellationToken cancellationToken);
}

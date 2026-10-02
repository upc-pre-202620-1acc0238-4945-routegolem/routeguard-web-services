using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using RouteGuard.Platform.Stakeholder.Domain.Model.Entities;
using RouteGuard.Platform.Stakeholder.Domain.Model.ValueObjects;
using RouteGuard.Platform.Stakeholder.Domain.Repositories;

namespace RouteGuard.Platform.Stakeholder.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class StudentGroupRepository(AppDbContext context)
    : BaseRepository<StudentGroup>(context), IStudentGroupRepository
{
    public Task<StudentGroup?> FindByStudentGroupIdAsync(StudentGroupId studentGroupId,
        CancellationToken cancellationToken) =>
        Context.Set<StudentGroup>().FirstOrDefaultAsync(group => group.Id == studentGroupId, cancellationToken);
}

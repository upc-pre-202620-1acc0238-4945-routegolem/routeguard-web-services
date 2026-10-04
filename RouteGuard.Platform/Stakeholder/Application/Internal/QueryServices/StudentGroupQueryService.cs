using RouteGuard.Platform.Stakeholder.Application.QueryServices;
using RouteGuard.Platform.Stakeholder.Domain.Model.Entities;
using RouteGuard.Platform.Stakeholder.Domain.Model.Queries;
using RouteGuard.Platform.Stakeholder.Domain.Model.ValueObjects;
using RouteGuard.Platform.Stakeholder.Domain.Repositories;

namespace RouteGuard.Platform.Stakeholder.Application.Internal.QueryServices;

public class StudentGroupQueryService(IStudentGroupRepository studentGroupRepository) : IStudentGroupQueryService
{
    public Task<StudentGroup?> Handle(GetStudentGroupByIdQuery query, CancellationToken cancellationToken) =>
        studentGroupRepository.FindByStudentGroupIdAsync(new StudentGroupId(query.StudentGroupId), cancellationToken);

    public async Task<IEnumerable<StudentGroup>> Handle(GetAllStudentGroupsQuery query, CancellationToken cancellationToken) =>
        await studentGroupRepository.ListAsync(cancellationToken);
}
using RouteGuard.Platform.StakeholderAssetManagement.Application.QueryServices;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Entities;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Queries;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Repositories;

namespace RouteGuard.Platform.StakeholderAssetManagement.Application.Internal.QueryServices;

public class StudentGroupQueryService(IStudentGroupRepository studentGroupRepository) : IStudentGroupQueryService
{
    public Task<StudentGroup?> Handle(GetStudentGroupByIdQuery query, CancellationToken cancellationToken) =>
        studentGroupRepository.FindByStudentGroupIdAsync(new StudentGroupId(query.StudentGroupId), cancellationToken);

    public async Task<IEnumerable<StudentGroup>> Handle(GetAllStudentGroupsQuery query, CancellationToken cancellationToken) =>
        await studentGroupRepository.ListAsync(cancellationToken);
}
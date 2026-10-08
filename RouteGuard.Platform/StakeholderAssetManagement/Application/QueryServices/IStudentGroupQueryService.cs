using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Entities;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Queries;

namespace RouteGuard.Platform.StakeholderAssetManagement.Application.QueryServices;

public interface IStudentGroupQueryService
{
    Task<StudentGroup?> Handle(GetStudentGroupByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<StudentGroup>> Handle(GetAllStudentGroupsQuery query, CancellationToken cancellationToken);
}
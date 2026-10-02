using RouteGuard.Platform.Stakeholder.Domain.Model.Aggregates;
using RouteGuard.Platform.Stakeholder.Domain.Model.Entities;
using RouteGuard.Platform.Stakeholder.Domain.Model.Queries;

namespace RouteGuard.Platform.Stakeholder.Application.QueryServices;

public interface IStakeholderQueryService
{
    Task<Parent?> Handle(GetParentByIdQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<Parent>> Handle(GetAllParentsQuery query, CancellationToken cancellationToken);

    Task<Driver?> Handle(GetDriverByIdQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<Driver>> Handle(GetAllDriversQuery query, CancellationToken cancellationToken);

    Task<StudentGroup?> Handle(GetStudentGroupByIdQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<StudentGroup>> Handle(GetAllStudentGroupsQuery query, CancellationToken cancellationToken);
}

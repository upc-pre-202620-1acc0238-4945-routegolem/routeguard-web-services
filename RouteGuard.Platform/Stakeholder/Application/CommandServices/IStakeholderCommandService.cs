using RouteGuard.Platform.Shared.Application.Model;
using RouteGuard.Platform.Stakeholder.Domain.Model.Aggregates;
using RouteGuard.Platform.Stakeholder.Domain.Model.Commands;
using RouteGuard.Platform.Stakeholder.Domain.Model.Entities;

namespace RouteGuard.Platform.Stakeholder.Application.CommandServices;

public interface IStakeholderCommandService
{
    Task<Result<Parent>> Handle(CreateParentCommand command, CancellationToken cancellationToken);

    Task<Result<Parent>> Handle(UpdateParentCommand command, CancellationToken cancellationToken);

    Task<Result<Parent>> Handle(DeleteParentCommand command, CancellationToken cancellationToken);

    Task<Result<Parent>> Handle(AddChildToParentCommand command, CancellationToken cancellationToken);

    Task<Result<Parent>> Handle(RemoveChildFromParentCommand command, CancellationToken cancellationToken);

    Task<Result<Driver>> Handle(CreateDriverCommand command, CancellationToken cancellationToken);

    Task<Result<Driver>> Handle(UpdateDriverCommand command, CancellationToken cancellationToken);

    Task<Result<Driver>> Handle(DeleteDriverCommand command, CancellationToken cancellationToken);

    Task<Result<Driver>> Handle(UpdateDriverPhoneCommand command, CancellationToken cancellationToken);

    Task<Result<StudentGroup>> Handle(CreateStudentGroupCommand command, CancellationToken cancellationToken);

    Task<Result<StudentGroup>> Handle(AddChildToGroupCommand command, CancellationToken cancellationToken);

    Task<Result<StudentGroup>> Handle(RemoveChildFromGroupCommand command, CancellationToken cancellationToken);

    Task<Result<StudentGroup>> Handle(FinalizeStudentGroupCommand command, CancellationToken cancellationToken);
}

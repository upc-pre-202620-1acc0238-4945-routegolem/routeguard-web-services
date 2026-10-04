using RouteGuard.Platform.Shared.Application.Model;
using RouteGuard.Platform.Stakeholder.Domain.Model.Commands;
using RouteGuard.Platform.Stakeholder.Domain.Model.Entities;

namespace RouteGuard.Platform.Stakeholder.Application.CommandServices;

public interface IStudentGroupCommandService
{
    Task<Result<StudentGroup>> Handle(CreateStudentGroupCommand command, CancellationToken cancellationToken);
    Task<Result<StudentGroup>> Handle(AddChildToGroupCommand command, CancellationToken cancellationToken);
    Task<Result<StudentGroup>> Handle(RemoveChildFromGroupCommand command, CancellationToken cancellationToken);
    Task<Result<StudentGroup>> Handle(FinalizeStudentGroupCommand command, CancellationToken cancellationToken);
}
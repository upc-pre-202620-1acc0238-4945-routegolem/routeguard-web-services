using RouteGuard.Platform.Shared.Application.Model;
using RouteGuard.Platform.Stakeholder.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Domain.Repositories;
using RouteGuard.Platform.Stakeholder.Application.CommandServices;
using RouteGuard.Platform.Stakeholder.Domain.Model;
using RouteGuard.Platform.Stakeholder.Domain.Model.Commands;
using RouteGuard.Platform.Stakeholder.Domain.Model.Entities;
using RouteGuard.Platform.Stakeholder.Domain.Repositories;

namespace RouteGuard.Platform.Stakeholder.Application.Internal.CommandServices;

public class StudentGroupCommandService(
    IStudentGroupRepository studentGroupRepository,
    IUnitOfWork unitOfWork) : IStudentGroupCommandService
{
    public async Task<Result<StudentGroup>> Handle(CreateStudentGroupCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var group = new StudentGroup(new OrganizationId(command.OrganizationId), command.Name);
            await studentGroupRepository.AddAsync(group, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<StudentGroup>.Success(group);
        }
        catch (ArgumentException ex) { return Result<StudentGroup>.Failure(StakeholderError.InvalidStakeholderData, ex.Message); }
    }

    public Task<Result<StudentGroup>> Handle(AddChildToGroupCommand command, CancellationToken cancellationToken) =>
        MutateGroupAsync(command.StudentGroupId, group => group.AddChild(new ChildId(command.ChildId)), cancellationToken);

    public Task<Result<StudentGroup>> Handle(RemoveChildFromGroupCommand command, CancellationToken cancellationToken) =>
        MutateGroupAsync(command.StudentGroupId, group => group.RemoveChild(new ChildId(command.ChildId)), cancellationToken);

    public Task<Result<StudentGroup>> Handle(FinalizeStudentGroupCommand command, CancellationToken cancellationToken) =>
        MutateGroupAsync(command.StudentGroupId, group => group.Finalize(), cancellationToken);

    private async Task<Result<StudentGroup>> MutateGroupAsync(Guid groupId, Action<StudentGroup> mutation, CancellationToken cancellationToken)
    {
        try
        {
            var group = await studentGroupRepository.FindByStudentGroupIdAsync(new StudentGroupId(groupId), cancellationToken);
            if (group is null) return Result<StudentGroup>.Failure(StakeholderError.StudentGroupNotFound, "Student group was not found.");
            mutation(group);
            studentGroupRepository.Update(group);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<StudentGroup>.Success(group);
        }
        catch (InvalidOperationException ex) { return Result<StudentGroup>.Failure(StakeholderError.InvalidStudentGroupState, ex.Message); }
    }
}
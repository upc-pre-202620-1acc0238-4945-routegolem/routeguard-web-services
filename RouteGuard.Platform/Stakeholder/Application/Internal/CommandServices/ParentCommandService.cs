using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.Shared.Application.Model;
using RouteGuard.Platform.Shared.Domain.Model.ValueObjects;
using RouteGuard.Platform.Shared.Domain.Repositories;
using RouteGuard.Platform.Stakeholder.Application.CommandServices;
using RouteGuard.Platform.Stakeholder.Domain.Model;
using RouteGuard.Platform.Stakeholder.Domain.Model.Aggregates;
using RouteGuard.Platform.Stakeholder.Domain.Model.Commands;
using RouteGuard.Platform.Stakeholder.Domain.Model.Entities;
using RouteGuard.Platform.Stakeholder.Domain.Model.ValueObjects;
using RouteGuard.Platform.Stakeholder.Domain.Repositories;

namespace RouteGuard.Platform.Stakeholder.Application.Internal.CommandServices;

public class ParentCommandService(
    IParentRepository parentRepository,
    IUnitOfWork unitOfWork) : IParentCommandService
{
    public async Task<Result<Parent>> Handle(CreateParentCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var parent = new Parent(command);
            await parentRepository.AddAsync(parent, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<Parent>.Success(parent);
        }
        catch (ArgumentException ex) { return Result<Parent>.Failure(StakeholderError.InvalidStakeholderData, ex.Message); }
        catch (DbUpdateException) { return Result<Parent>.Failure(StakeholderError.DatabaseError, "Database error."); }
    }

    public async Task<Result<Parent>> Handle(UpdateParentCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var parent = await parentRepository.FindByParentIdAsync(new ParentId(command.ParentId), cancellationToken);
            if (parent is null) return Result<Parent>.Failure(StakeholderError.ParentNotFound, "Parent was not found.");
            parent.Update(new FullName(command.FirstName, command.LastName), new Email(command.Email), new PhoneNumber(command.PhoneNumber));
            parentRepository.Update(parent);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<Parent>.Success(parent);
        }
        catch (ArgumentException ex) { return Result<Parent>.Failure(StakeholderError.InvalidStakeholderData, ex.Message); }
    }

    public async Task<Result<Parent>> Handle(DeleteParentCommand command, CancellationToken cancellationToken)
    {
        var parent = await parentRepository.FindByParentIdAsync(new ParentId(command.ParentId), cancellationToken);
        if (parent is null) return Result<Parent>.Failure(StakeholderError.ParentNotFound, "Parent was not found.");
        parentRepository.Remove(parent);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<Parent>.Success(parent);
    }

    public Task<Result<Parent>> Handle(AddChildToParentCommand command, CancellationToken cancellationToken) =>
        MutateParentAsync(command.ParentId, parent => parent.AddChild(new Child(new FullName(command.FirstName, command.LastName), command.Age)), cancellationToken);

    public Task<Result<Parent>> Handle(RemoveChildFromParentCommand command, CancellationToken cancellationToken) =>
        MutateParentAsync(command.ParentId, parent => parent.RemoveChild(new ChildId(command.ChildId)), cancellationToken);

    private async Task<Result<Parent>> MutateParentAsync(Guid parentId, Action<Parent> mutation, CancellationToken cancellationToken)
    {
        try
        {
            var parent = await parentRepository.FindByParentIdAsync(new ParentId(parentId), cancellationToken);
            if (parent is null) return Result<Parent>.Failure(StakeholderError.ParentNotFound, "Parent was not found.");
            mutation(parent);
            parentRepository.Update(parent);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<Parent>.Success(parent);
        }
        catch (ArgumentException ex) { return Result<Parent>.Failure(StakeholderError.InvalidStakeholderData, ex.Message); }
    }
}
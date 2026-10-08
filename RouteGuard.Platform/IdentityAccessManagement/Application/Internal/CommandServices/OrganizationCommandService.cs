using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.IdentityAccessManagement.Application.CommandServices;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.Commands;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Repositories;
using RouteGuard.Platform.Shared.Application.Model;
using RouteGuard.Platform.Shared.Domain.Repositories;

namespace RouteGuard.Platform.IdentityAccessManagement.Application.Internal.CommandServices;

public class OrganizationCommandService(
    IOrganizationRepository organizationRepository,
    IUnitOfWork unitOfWork) : IOrganizationCommandService
{
    public async Task<Result<Organization>> Handle(CreateOrganizationCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var organization = new Organization(new OrganizationName(command.Name));
            await organizationRepository.AddAsync(organization, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<Organization>.Success(organization);
        }
        catch (ArgumentException ex) { return Result<Organization>.Failure(IamError.InvalidIamData, ex.Message); }
        catch (DbUpdateException) { return Result<Organization>.Failure(IamError.DatabaseError, "Database error."); }
    }

    public async Task<Result<Organization>> Handle(UpdateOrganizationCommand command, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.FindByOrganizationIdAsync(new OrganizationId(command.OrganizationId), cancellationToken);
        if (organization is null) return Result<Organization>.Failure(IamError.OrganizationNotFound, "Organization was not found.");

        try
        {
            organization.UpdateName(new OrganizationName(command.Name));
            organizationRepository.Update(organization);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<Organization>.Success(organization);
        }
        catch (ArgumentException ex) { return Result<Organization>.Failure(IamError.InvalidIamData, ex.Message); }
    }
}
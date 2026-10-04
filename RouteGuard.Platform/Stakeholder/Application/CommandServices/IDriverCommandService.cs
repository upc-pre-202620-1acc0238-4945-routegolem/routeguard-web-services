using RouteGuard.Platform.Shared.Application.Model;
using RouteGuard.Platform.Stakeholder.Domain.Model.Commands;
using RouteGuard.Platform.Stakeholder.Domain.Model.Entities;

namespace RouteGuard.Platform.Stakeholder.Application.CommandServices;

public interface IDriverCommandService
{
    Task<Result<Driver>> Handle(CreateDriverCommand command, CancellationToken cancellationToken);
    Task<Result<Driver>> Handle(UpdateDriverCommand command, CancellationToken cancellationToken);
    Task<Result<Driver>> Handle(DeleteDriverCommand command, CancellationToken cancellationToken);
    Task<Result<Driver>> Handle(UpdateDriverPhoneCommand command, CancellationToken cancellationToken);
}
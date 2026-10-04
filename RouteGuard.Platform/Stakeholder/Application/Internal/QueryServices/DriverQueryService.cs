using RouteGuard.Platform.Stakeholder.Application.QueryServices;
using RouteGuard.Platform.Stakeholder.Domain.Model.Entities;
using RouteGuard.Platform.Stakeholder.Domain.Model.Queries;
using RouteGuard.Platform.Stakeholder.Domain.Model.ValueObjects;
using RouteGuard.Platform.Stakeholder.Domain.Repositories;

namespace RouteGuard.Platform.Stakeholder.Application.Internal.QueryServices;

public class DriverQueryService(IDriverRepository driverRepository) : IDriverQueryService
{
    public Task<Driver?> Handle(GetDriverByIdQuery query, CancellationToken cancellationToken) =>
        driverRepository.FindByDriverIdAsync(new DriverId(query.DriverId), cancellationToken);

    public async Task<IEnumerable<Driver>> Handle(GetAllDriversQuery query, CancellationToken cancellationToken) =>
        await driverRepository.ListAsync(cancellationToken);
}
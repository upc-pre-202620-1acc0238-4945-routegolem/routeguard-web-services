using RouteGuard.Platform.Shared.Application.Model;
using RouteGuard.Platform.Shared.Domain.Repositories;
using RouteGuard.Platform.StakeholderAssetManagement.Application.CommandServices;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Commands;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Entities;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.ValueObjects;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Repositories;

using DriverId = RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.ValueObjects.DriverId;

namespace RouteGuard.Platform.StakeholderAssetManagement.Application.Internal.CommandServices;

public class DriverCommandService(
    IDriverRepository driverRepository,
    IUnitOfWork unitOfWork) : IDriverCommandService
{
    public async Task<Result<Driver>> Handle(CreateDriverCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var driver = new Driver(command);
            await driverRepository.AddAsync(driver, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<Driver>.Success(driver);
        }
        catch (ArgumentException ex) { return Result<Driver>.Failure(StakeholderError.InvalidStakeholderData, ex.Message); }
    }

    public async Task<Result<Driver>> Handle(UpdateDriverCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var driver = await driverRepository.FindByDriverIdAsync(new DriverId(command.DriverId), cancellationToken);
            if (driver is null) return Result<Driver>.Failure(StakeholderError.DriverNotFound, "Driver was not found.");
            driver.Update(new FullName(command.FirstName, command.LastName), new Email(command.Email),
                new PhoneNumber(command.PhoneNumber), new LicenseNumber(command.LicenseNumber), command.Available);
            driverRepository.Update(driver);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<Driver>.Success(driver);
        }
        catch (ArgumentException ex) { return Result<Driver>.Failure(StakeholderError.InvalidStakeholderData, ex.Message); }
    }

    public async Task<Result<Driver>> Handle(DeleteDriverCommand command, CancellationToken cancellationToken)
    {
        var driver = await driverRepository.FindByDriverIdAsync(new DriverId(command.DriverId), cancellationToken);
        if (driver is null) return Result<Driver>.Failure(StakeholderError.DriverNotFound, "Driver was not found.");
        driverRepository.Remove(driver);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<Driver>.Success(driver);
    }

    public async Task<Result<Driver>> Handle(UpdateDriverPhoneCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var driver = await driverRepository.FindByDriverIdAsync(new DriverId(command.DriverId), cancellationToken);
            if (driver is null) return Result<Driver>.Failure(StakeholderError.DriverNotFound, "Driver was not found.");
            driver.UpdatePhoneNumber(new PhoneNumber(command.PhoneNumber));
            driverRepository.Update(driver);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<Driver>.Success(driver);
        }
        catch (ArgumentException ex) { return Result<Driver>.Failure(StakeholderError.InvalidStakeholderData, ex.Message); }
    }
}
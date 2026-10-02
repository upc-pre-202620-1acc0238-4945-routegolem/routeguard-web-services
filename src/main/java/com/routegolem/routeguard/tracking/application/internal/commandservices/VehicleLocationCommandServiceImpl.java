package com.routegolem.routeguard.tracking.application.internal.commandservices;

import com.routegolem.routeguard.shared.application.ApplicationError;
import com.routegolem.routeguard.shared.application.Result;
import com.routegolem.routeguard.tracking.application.commandservices.VehicleLocationCommandService;
import com.routegolem.routeguard.tracking.domain.model.aggregates.VehicleLocation;
import com.routegolem.routeguard.tracking.domain.model.commands.UpdateVehicleLocationCommand;
import com.routegolem.routeguard.tracking.domain.model.valueobjects.Coordinates;
import com.routegolem.routeguard.tracking.domain.repositories.VehicleLocationRepository;
import org.springframework.stereotype.Service;

@Service
public class VehicleLocationCommandServiceImpl implements VehicleLocationCommandService {

    private final VehicleLocationRepository repository;

    public VehicleLocationCommandServiceImpl(VehicleLocationRepository repository) {
        this.repository = repository;
    }

    @Override
    public Result<VehicleLocation, ApplicationError> handle(UpdateVehicleLocationCommand command){
        var newCoordinates = new Coordinates(command.latitude(), command.longitude());

        // We seek if there's another location registered for this vehicle.
        var existingLocation = repository.findByVehicleId(command.vehicleId());

        VehicleLocation vehicleLocationToSave;

        if (existingLocation.isPresent()) {
            // If it exists, we update its coordinates using the Domain's business method
            vehicleLocationToSave = existingLocation.get();
            vehicleLocationToSave.updateLocation(newCoordinates);
        } else {
            // If it does not exist, we create a new one
            vehicleLocationToSave = new VehicleLocation(command.vehicleId(), newCoordinates);
        }

        try {
            var savedLocation = repository.save(vehicleLocationToSave);
            return Result.success(savedLocation);
        } catch (Exception e) {
            return Result.failure(ApplicationError.unexpected("update-location", e.getMessage()));
        }
    }
}

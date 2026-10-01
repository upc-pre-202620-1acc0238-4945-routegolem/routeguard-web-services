package com.routegolem.routeguard.tracking.domain.model.commands;

public record UpdateVehicleLocationCommand(Long vehicleId, Double latitude, Double longitude) {
    public UpdateVehicleLocationCommand {
        if (vehicleId == null || vehicleId <= 0) {
            throw new IllegalArgumentException("VehicleId cannot be null or less than or equal to 0.");
        }
        if (latitude == null || longitude == null) {
            throw new IllegalArgumentException("Latitude and longitude cannot be null.");
        }
    }
}

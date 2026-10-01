package com.routegolem.routeguard.tracking.domain.model.queries;

public record GetVehicleLocationByVehicleIdQuery(Long vehicleId) {
    public GetVehicleLocationByVehicleIdQuery {
        if (vehicleId == null || vehicleId <= 0) {
            throw new IllegalArgumentException("Vehicle Id cannot be null or less than or equal to zero.");
        }
    }
}

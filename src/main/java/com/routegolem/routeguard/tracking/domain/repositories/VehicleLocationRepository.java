package com.routegolem.routeguard.tracking.domain.repositories;

import com.routegolem.routeguard.tracking.domain.model.aggregates.VehicleLocation;
import java.util.Optional;

public interface VehicleLocationRepository {
    Optional<VehicleLocation> findByVehicleId(Long vehicleId);
    VehicleLocation save(VehicleLocation vehicleLocation);
}

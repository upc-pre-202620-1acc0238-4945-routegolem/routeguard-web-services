package com.routegolem.routeguard.tracking.interfaces.rest.transform;

import com.routegolem.routeguard.tracking.domain.model.aggregates.VehicleLocation;
import com.routegolem.routeguard.tracking.interfaces.rest.resources.VehicleLocationResource;

public class VehicleLocationResourceFromEntityAssembler {
    public static VehicleLocationResource toResourceFromEntity(VehicleLocation entity) {
        return new VehicleLocationResource(
                entity.getVehicleId(),
                entity.getCurrentCoordinates().latitude(),
                entity.getCurrentCoordinates().longitude(),
                entity.getLastUpdatedAt()
        );
    }
}

package com.routegolem.routeguard.tracking.interfaces.rest.transform;

import com.routegolem.routeguard.tracking.domain.model.commands.UpdateVehicleLocationCommand;
import com.routegolem.routeguard.tracking.interfaces.rest.resources.UpdateVehicleLocationResource;

public class UpdateVehicleLocationCommandFromResourceAssembler {
    public static UpdateVehicleLocationCommand toCommandFromResource(UpdateVehicleLocationResource resource) {
        return new UpdateVehicleLocationCommand(resource.vehicleId(), resource.latitude(), resource.longitude());
    }
}

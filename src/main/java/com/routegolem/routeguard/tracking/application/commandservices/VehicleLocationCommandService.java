package com.routegolem.routeguard.tracking.application.commandservices;

import com.routegolem.routeguard.shared.application.ApplicationError;
import com.routegolem.routeguard.shared.application.Result;
import com.routegolem.routeguard.tracking.domain.model.aggregates.VehicleLocation;
import com.routegolem.routeguard.tracking.domain.model.commands.UpdateVehicleLocationCommand;

public interface VehicleLocationCommandService {
    Result<VehicleLocation, ApplicationError> handle(UpdateVehicleLocationCommand command);
}

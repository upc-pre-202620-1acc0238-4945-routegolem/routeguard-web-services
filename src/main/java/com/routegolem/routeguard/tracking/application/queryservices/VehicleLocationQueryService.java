package com.routegolem.routeguard.tracking.application.queryservices;

import com.routegolem.routeguard.tracking.domain.model.aggregates.VehicleLocation;
import com.routegolem.routeguard.tracking.domain.model.queries.GetVehicleLocationByVehicleIdQuery;

import java.util.Optional;

public interface VehicleLocationQueryService {
    Optional<VehicleLocation> handle(GetVehicleLocationByVehicleIdQuery query);
}

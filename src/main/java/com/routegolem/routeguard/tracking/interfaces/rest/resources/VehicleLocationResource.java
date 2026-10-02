package com.routegolem.routeguard.tracking.interfaces.rest.resources;

import java.time.LocalDateTime;

public record VehicleLocationResource(
        Long vehicleId,
        Double latitude,
        Double longitude,
        LocalDateTime lastUpdatedAt
) {}

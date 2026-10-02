package com.routegolem.routeguard.tracking.interfaces.rest.resources;

import jakarta.validation.constraints.NotNull;

public record UpdateVehicleLocationResource(
        @NotNull Long vehicleId,
        @NotNull Double latitude,
        @NotNull Double longitude
) {}

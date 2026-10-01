package com.routegolem.routeguard.tracking.domain.model.aggregates;

import com.routegolem.routeguard.shared.domain.model.aggregates.AbstractDomainAggregateRoot;
import com.routegolem.routeguard.tracking.domain.model.valueobjects.Coordinates;
import lombok.Getter;

import java.time.LocalDateTime;

@Getter
public class VehicleLocation extends AbstractDomainAggregateRoot<VehicleLocation> {

    private Long id;
    private Long vehicleId; // bus ID (reference to Fleet Domain)
    private Coordinates currentCoordinates;
    private LocalDateTime lastUpdatedAt;

    public VehicleLocation(Long vehicleId, Coordinates initialCoordinates) {
        this.vehicleId = vehicleId;
        this.currentCoordinates = initialCoordinates;
        this.lastUpdatedAt = LocalDateTime.now();
    }

    // Business Logic: Update Location
    public void updateLocation(Coordinates newCoordinates) {
        this.currentCoordinates = newCoordinates;
        this.lastUpdatedAt = LocalDateTime.now();
    }
}

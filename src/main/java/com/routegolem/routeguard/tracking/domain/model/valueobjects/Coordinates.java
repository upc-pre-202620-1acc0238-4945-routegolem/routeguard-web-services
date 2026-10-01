package com.routegolem.routeguard.tracking.domain.model.valueobjects;

public record Coordinates(Double longitude, Double latitude) {
    public Coordinates {
        if (latitude == null || latitude < -90 || latitude > 90) {
            throw new IllegalArgumentException("Latitude must be valid (between -90 and 90).");
        }
        if (longitude == null || longitude < -180 || longitude > 180) {
            throw new IllegalArgumentException("Longitude must be valid (between -180 and 180).");
        }
    }
}

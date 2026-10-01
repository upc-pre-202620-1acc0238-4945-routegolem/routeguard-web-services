package com.routegolem.routeguard.tracking.infrastructure.persistence.jpa.repositories;

import com.routegolem.routeguard.tracking.infrastructure.persistence.jpa.entities.VehicleLocationPersistenceEntity;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.Optional;

@Repository
public interface VehicleLocationPersistenceRepository extends JpaRepository<VehicleLocationPersistenceEntity, Long> {
    // Spring Boot create the SQL query automatically based on this name:
    Optional<VehicleLocationPersistenceEntity> findByVehicleId(Long vehicleId);
}

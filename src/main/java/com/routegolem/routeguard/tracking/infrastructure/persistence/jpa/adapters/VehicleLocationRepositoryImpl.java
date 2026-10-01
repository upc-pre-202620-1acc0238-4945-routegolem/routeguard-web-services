package com.routegolem.routeguard.tracking.infrastructure.persistence.jpa.adapters;

import com.routegolem.routeguard.tracking.domain.model.aggregates.VehicleLocation;
import com.routegolem.routeguard.tracking.domain.repositories.VehicleLocationRepository;
import com.routegolem.routeguard.tracking.infrastructure.persistence.jpa.assemblers.VehicleLocationPersistenceAssembler;
import com.routegolem.routeguard.tracking.infrastructure.persistence.jpa.repositories.VehicleLocationPersistenceRepository;
import org.springframework.stereotype.Repository;

import java.util.Optional;

@Repository
public class VehicleLocationRepositoryImpl implements VehicleLocationRepository {
    private final VehicleLocationPersistenceRepository persistenceRepository;

    public VehicleLocationRepositoryImpl(VehicleLocationPersistenceRepository persistenceRepository) {
        this.persistenceRepository = persistenceRepository;
    }

    @Override
    public Optional<VehicleLocation> findByVehicleId(Long vehicleId) {
        return persistenceRepository.findByVehicleId(vehicleId)
                .map(VehicleLocationPersistenceAssembler::toDomainFromPersistence);
    }

    @Override
    public VehicleLocation save(VehicleLocation vehicleLocation) {
        var entityToSave = VehicleLocationPersistenceAssembler.toPersistenceFromDomain(vehicleLocation);
        var savedEntity = persistenceRepository.save(entityToSave);
        return VehicleLocationPersistenceAssembler.toDomainFromPersistence(savedEntity);
    }
}

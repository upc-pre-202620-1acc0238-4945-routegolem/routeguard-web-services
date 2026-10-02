package com.routegolem.routeguard.tracking.application.internal.queryservices;

import com.routegolem.routeguard.tracking.application.queryservices.VehicleLocationQueryService;
import com.routegolem.routeguard.tracking.domain.model.aggregates.VehicleLocation;
import com.routegolem.routeguard.tracking.domain.model.queries.GetVehicleLocationByVehicleIdQuery;
import com.routegolem.routeguard.tracking.domain.repositories.VehicleLocationRepository;
import org.springframework.stereotype.Service;

import java.util.Optional;

@Service
public class VehicleLocationQueryServiceImpl implements VehicleLocationQueryService {

    private final VehicleLocationRepository repository;

    public VehicleLocationQueryServiceImpl(VehicleLocationRepository repository) {
        this.repository = repository;
    }

    @Override
    public Optional<VehicleLocation> handle(GetVehicleLocationByVehicleIdQuery query) {
        return repository.findByVehicleId(query.vehicleId());
    }
}

package com.routegolem.routeguard.tracking.interfaces.rest;

import com.routegolem.routeguard.shared.application.ApplicationError;
import com.routegolem.routeguard.shared.application.Result;
import com.routegolem.routeguard.tracking.application.commandservices.VehicleLocationCommandService;
import com.routegolem.routeguard.tracking.application.queryservices.VehicleLocationQueryService;
import com.routegolem.routeguard.tracking.domain.model.aggregates.VehicleLocation;
import com.routegolem.routeguard.tracking.domain.model.queries.GetVehicleLocationByVehicleIdQuery;
import com.routegolem.routeguard.tracking.interfaces.rest.resources.UpdateVehicleLocationResource;
import com.routegolem.routeguard.tracking.interfaces.rest.resources.VehicleLocationResource;
import com.routegolem.routeguard.tracking.interfaces.rest.transform.UpdateVehicleLocationCommandFromResourceAssembler;
import com.routegolem.routeguard.tracking.interfaces.rest.transform.VehicleLocationResourceFromEntityAssembler;
import jakarta.validation.Valid;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/v1/vehicle-locations")
public class VehicleLocationController {

    private final VehicleLocationCommandService commandService;
    private final VehicleLocationQueryService queryService;

    public VehicleLocationController(VehicleLocationCommandService commandService, VehicleLocationQueryService queryService) {
        this.commandService = commandService;
        this.queryService = queryService;
    }

    // The bus sends its coordinates through here
    @PostMapping
    public ResponseEntity<?> updateLocation(@RequestBody @Valid UpdateVehicleLocationResource resource) {
        var command = UpdateVehicleLocationCommandFromResourceAssembler.toCommandFromResource(resource);
        var result = commandService.handle(command);

        // Unpack the Result (Java's Pattern Matching)
        if (result instanceof Result.Success<?, ?> success) {
            var entity = (VehicleLocation) success.value();
            var responseResource = VehicleLocationResourceFromEntityAssembler.toResourceFromEntity(entity);
            return ResponseEntity.status(HttpStatus.CREATED).body(responseResource);
        } else {
            var failure = (Result.Failure<?, ?>) result;
            return ResponseEntity.badRequest().body(failure.error());
        }
    }

    // The parents query the bus' location through here
    @GetMapping("/{vehicleId}")
    public ResponseEntity<VehicleLocationResource> getLocationByVehicleId(@PathVariable Long vehicleId) {
        var query = new GetVehicleLocationByVehicleIdQuery(vehicleId);
        var location = queryService.handle(query);

        if (location.isEmpty()) {
            return ResponseEntity.notFound().build();
        }

        var responseResource = VehicleLocationResourceFromEntityAssembler.toResourceFromEntity(location.get());
        return ResponseEntity.ok(responseResource);
    }
}

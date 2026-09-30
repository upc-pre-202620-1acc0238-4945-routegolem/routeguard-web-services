package com.routegolem.routeguard.iam.interfaces.rest;
import com.routegolem.routeguard.iam.domain.model.queries.GetAllRolesQuery;
import com.routegolem.routeguard.iam.domain.services.RoleQueryService;
import com.routegolem.routeguard.iam.interfaces.rest.resources.RoleResource;
import com.routegolem.routeguard.iam.interfaces.rest.transform.RoleResourceFromEntityAssembler;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import java.util.List;
import java.util.stream.Collectors;
@RestController
@RequestMapping("/api/v1/roles")
public class RolesController {
    private final RoleQueryService roleQueryService;
    public RolesController(RoleQueryService roleQueryService) { this.roleQueryService = roleQueryService; }
    @GetMapping
    public ResponseEntity<List<RoleResource>> getAllRoles() {
        var roles = roleQueryService.handle(new GetAllRolesQuery());
        var resources = roles.stream().map(RoleResourceFromEntityAssembler::toResourceFromEntity).collect(Collectors.toList());
        return ResponseEntity.ok(resources);
    }
}

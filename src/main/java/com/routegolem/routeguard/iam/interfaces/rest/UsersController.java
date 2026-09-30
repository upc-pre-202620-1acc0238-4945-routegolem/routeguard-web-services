package com.routegolem.routeguard.iam.interfaces.rest;
import com.routegolem.routeguard.iam.domain.model.queries.GetAllUsersQuery;
import com.routegolem.routeguard.iam.domain.model.queries.GetUserByIdQuery;
import com.routegolem.routeguard.iam.domain.services.UserQueryService;
import com.routegolem.routeguard.iam.interfaces.rest.resources.UserResource;
import com.routegolem.routeguard.iam.interfaces.rest.transform.UserResourceFromEntityAssembler;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import java.util.List;
import java.util.stream.Collectors;
@RestController
@RequestMapping("/api/v1/users")
public class UsersController {
    private final UserQueryService userQueryService;
    public UsersController(UserQueryService userQueryService) { this.userQueryService = userQueryService; }
    @GetMapping
    public ResponseEntity<List<UserResource>> getAllUsers() {
        var users = userQueryService.handle(new GetAllUsersQuery());
        var resources = users.stream().map(UserResourceFromEntityAssembler::toResourceFromEntity).collect(Collectors.toList());
        return ResponseEntity.ok(resources);
    }
    @GetMapping("/{id}")
    public ResponseEntity<UserResource> getUserById(@PathVariable Long id) {
        var user = userQueryService.handle(new GetUserByIdQuery(id));
        if (user.isEmpty()) return ResponseEntity.notFound().build();
        return ResponseEntity.ok(UserResourceFromEntityAssembler.toResourceFromEntity(user.get()));
    }
}

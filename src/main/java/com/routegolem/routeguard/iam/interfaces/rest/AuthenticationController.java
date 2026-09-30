package com.routegolem.routeguard.iam.interfaces.rest;
import com.routegolem.routeguard.iam.domain.model.aggregates.User;
import com.routegolem.routeguard.iam.domain.services.UserCommandService;
import com.routegolem.routeguard.iam.interfaces.rest.resources.AuthenticatedUserResource;
import com.routegolem.routeguard.iam.interfaces.rest.resources.SignInResource;
import com.routegolem.routeguard.iam.interfaces.rest.resources.SignUpResource;
import com.routegolem.routeguard.iam.interfaces.rest.resources.UserResource;
import com.routegolem.routeguard.iam.interfaces.rest.transform.AuthenticatedUserResourceFromEntityAssembler;
import com.routegolem.routeguard.iam.interfaces.rest.transform.SignInCommandFromResourceAssembler;
import com.routegolem.routeguard.iam.interfaces.rest.transform.SignUpCommandFromResourceAssembler;
import com.routegolem.routeguard.iam.interfaces.rest.transform.UserResourceFromEntityAssembler;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
@RestController
@RequestMapping("/api/v1/authentication")
public class AuthenticationController {
    private final UserCommandService userCommandService;
    public AuthenticationController(UserCommandService userCommandService) { this.userCommandService = userCommandService; }
    @PostMapping("/sign-in")
    public ResponseEntity<AuthenticatedUserResource> signIn(@RequestBody SignInResource resource) {
        var result = userCommandService.handle(SignInCommandFromResourceAssembler.toCommandFromResource(resource));
        if (result.isEmpty()) return ResponseEntity.status(HttpStatus.UNAUTHORIZED).build();
        return ResponseEntity.ok(AuthenticatedUserResourceFromEntityAssembler.toResourceFromEntity(result.get().getLeft(), result.get().getRight()));
    }
    @PostMapping("/sign-up")
    public ResponseEntity<UserResource> signUp(@RequestBody SignUpResource resource) {
        var user = userCommandService.handle(SignUpCommandFromResourceAssembler.toCommandFromResource(resource));
        if (user.isEmpty()) return ResponseEntity.badRequest().build();
        return ResponseEntity.status(HttpStatus.CREATED).body(UserResourceFromEntityAssembler.toResourceFromEntity(user.get()));
    }
}

package com.routegolem.routeguard.iam.interfaces.rest.transform;
import com.routegolem.routeguard.iam.domain.model.commands.SignUpCommand;
import com.routegolem.routeguard.iam.interfaces.rest.resources.SignUpResource;
public class SignUpCommandFromResourceAssembler {
    public static SignUpCommand toCommandFromResource(SignUpResource resource) {
        return new SignUpCommand(resource.username(), resource.password(), resource.roles() != null ? resource.roles() : java.util.List.of());
    }
}

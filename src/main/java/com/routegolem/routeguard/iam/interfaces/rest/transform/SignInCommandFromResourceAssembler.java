package com.routegolem.routeguard.iam.interfaces.rest.transform;
import com.routegolem.routeguard.iam.domain.model.commands.SignInCommand;
import com.routegolem.routeguard.iam.interfaces.rest.resources.SignInResource;
public class SignInCommandFromResourceAssembler {
    public static SignInCommand toCommandFromResource(SignInResource resource) {
        return new SignInCommand(resource.username(), resource.password());
    }
}

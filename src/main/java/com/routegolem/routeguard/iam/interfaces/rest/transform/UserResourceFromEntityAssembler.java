package com.routegolem.routeguard.iam.interfaces.rest.transform;
import com.routegolem.routeguard.iam.domain.model.aggregates.User;
import com.routegolem.routeguard.iam.interfaces.rest.resources.UserResource;
import java.util.stream.Collectors;
public class UserResourceFromEntityAssembler {
    public static UserResource toResourceFromEntity(User entity) {
        return new UserResource(entity.getId(), entity.getUsername(),
                entity.getRoles().stream().map(r -> r.getName().name()).collect(Collectors.toList()));
    }
}

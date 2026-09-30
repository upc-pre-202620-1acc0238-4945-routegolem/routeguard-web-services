package com.routegolem.routeguard.iam.interfaces.rest.transform;
import com.routegolem.routeguard.iam.domain.model.entities.Role;
import com.routegolem.routeguard.iam.interfaces.rest.resources.RoleResource;
public class RoleResourceFromEntityAssembler {
    public static RoleResource toResourceFromEntity(Role entity) {
        return new RoleResource(entity.getId(), entity.getName().name());
    }
}

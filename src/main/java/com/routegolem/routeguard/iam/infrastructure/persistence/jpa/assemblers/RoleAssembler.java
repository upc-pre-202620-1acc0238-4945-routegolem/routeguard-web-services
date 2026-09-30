package com.routegolem.routeguard.iam.infrastructure.persistence.jpa.assemblers;
import com.routegolem.routeguard.iam.domain.model.entities.Role;
import com.routegolem.routeguard.iam.infrastructure.persistence.jpa.entities.RolePersistenceEntity;
public class RoleAssembler {
    public static Role toDomain(RolePersistenceEntity entity) {
        return new Role(entity.getId(), entity.getName());
    }
    public static RolePersistenceEntity toPersistence(Role domain) {
        RolePersistenceEntity entity = new RolePersistenceEntity();
        entity.setId(domain.getId());
        entity.setName(domain.getName());
        return entity;
    }
}

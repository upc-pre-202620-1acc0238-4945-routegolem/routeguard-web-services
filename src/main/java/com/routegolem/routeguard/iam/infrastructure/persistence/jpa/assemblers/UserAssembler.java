package com.routegolem.routeguard.iam.infrastructure.persistence.jpa.assemblers;
import com.routegolem.routeguard.iam.domain.model.aggregates.User;
import com.routegolem.routeguard.iam.infrastructure.persistence.jpa.entities.UserPersistenceEntity;
import java.util.stream.Collectors;
public class UserAssembler {
    public static User toDomain(UserPersistenceEntity entity) {
        return new User(entity.getId(), entity.getUsername(), entity.getPassword(),
                entity.getRoles().stream().map(RoleAssembler::toDomain).collect(Collectors.toSet()));
    }
    public static UserPersistenceEntity toPersistence(User domain) {
        UserPersistenceEntity entity = new UserPersistenceEntity();
        entity.setId(domain.getId());
        entity.setUsername(domain.getUsername());
        entity.setPassword(domain.getPassword());
        entity.setRoles(domain.getRoles().stream().map(RoleAssembler::toPersistence).collect(Collectors.toSet()));
        return entity;
    }
}

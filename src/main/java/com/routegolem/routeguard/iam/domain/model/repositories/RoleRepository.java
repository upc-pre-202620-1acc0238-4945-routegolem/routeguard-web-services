package com.routegolem.routeguard.iam.domain.model.repositories;
import com.routegolem.routeguard.iam.domain.model.entities.Role;
import com.routegolem.routeguard.iam.domain.model.valueobjects.Roles;
import java.util.Optional;
import java.util.List;
public interface RoleRepository {
    Optional<Role> findByName(Roles name);
    boolean existsByName(Roles name);
    Role save(Role role);
    List<Role> findAll();
}

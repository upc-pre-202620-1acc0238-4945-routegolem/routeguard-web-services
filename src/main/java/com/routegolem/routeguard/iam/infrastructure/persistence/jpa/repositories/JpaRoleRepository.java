package com.routegolem.routeguard.iam.infrastructure.persistence.jpa.repositories;
import com.routegolem.routeguard.iam.domain.model.valueobjects.Roles;
import com.routegolem.routeguard.iam.infrastructure.persistence.jpa.entities.RolePersistenceEntity;
import org.springframework.data.jpa.repository.JpaRepository;
import java.util.Optional;
public interface JpaRoleRepository extends JpaRepository<RolePersistenceEntity, Long> {
    Optional<RolePersistenceEntity> findByName(Roles name);
    boolean existsByName(Roles name);
}

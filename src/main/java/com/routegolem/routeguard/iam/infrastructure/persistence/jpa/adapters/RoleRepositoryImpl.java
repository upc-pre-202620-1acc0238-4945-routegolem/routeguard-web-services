package com.routegolem.routeguard.iam.infrastructure.persistence.jpa.adapters;
import com.routegolem.routeguard.iam.domain.model.entities.Role;
import com.routegolem.routeguard.iam.domain.model.repositories.RoleRepository;
import com.routegolem.routeguard.iam.domain.model.valueobjects.Roles;
import com.routegolem.routeguard.iam.infrastructure.persistence.jpa.assemblers.RoleAssembler;
import com.routegolem.routeguard.iam.infrastructure.persistence.jpa.repositories.JpaRoleRepository;
import org.springframework.stereotype.Repository;
import java.util.List;
import java.util.Optional;
import java.util.stream.Collectors;
@Repository
public class RoleRepositoryImpl implements RoleRepository {
    private final JpaRoleRepository jpaRoleRepository;
    public RoleRepositoryImpl(JpaRoleRepository jpaRoleRepository) { this.jpaRoleRepository = jpaRoleRepository; }
    @Override public Optional<Role> findByName(Roles name) { return jpaRoleRepository.findByName(name).map(RoleAssembler::toDomain); }
    @Override public boolean existsByName(Roles name) { return jpaRoleRepository.existsByName(name); }
    @Override public Role save(Role role) { return RoleAssembler.toDomain(jpaRoleRepository.save(RoleAssembler.toPersistence(role))); }
    @Override public List<Role> findAll() { return jpaRoleRepository.findAll().stream().map(RoleAssembler::toDomain).collect(Collectors.toList()); }
}

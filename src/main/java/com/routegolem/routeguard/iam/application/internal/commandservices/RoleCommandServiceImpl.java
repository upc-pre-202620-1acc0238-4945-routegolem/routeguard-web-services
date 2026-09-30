package com.routegolem.routeguard.iam.application.internal.commandservices;
import com.routegolem.routeguard.iam.domain.model.commands.SeedRolesCommand;
import com.routegolem.routeguard.iam.domain.model.entities.Role;
import com.routegolem.routeguard.iam.domain.model.repositories.RoleRepository;
import com.routegolem.routeguard.iam.domain.model.valueobjects.Roles;
import com.routegolem.routeguard.iam.domain.services.RoleCommandService;
import org.springframework.stereotype.Service;
@Service
public class RoleCommandServiceImpl implements RoleCommandService {
    private final RoleRepository roleRepository;
    public RoleCommandServiceImpl(RoleRepository roleRepository) { this.roleRepository = roleRepository; }
    @Override public void handle(SeedRolesCommand command) {
        for (Roles name : Roles.values()) {
            if (!roleRepository.existsByName(name)) {
                roleRepository.save(new Role(name));
            }
        }
    }
}

package com.routegolem.routeguard.iam.domain.services;
import com.routegolem.routeguard.iam.domain.model.commands.SeedRolesCommand;
public interface RoleCommandService {
    void handle(SeedRolesCommand command);
}

package com.routegolem.routeguard.iam.application.internal.eventhandlers;
import com.routegolem.routeguard.iam.domain.model.commands.SeedRolesCommand;
import com.routegolem.routeguard.iam.domain.services.RoleCommandService;
import org.springframework.boot.context.event.ApplicationReadyEvent;
import org.springframework.context.event.EventListener;
import org.springframework.stereotype.Service;
@Service
public class ApplicationReadyEventHandler {
    private final RoleCommandService roleCommandService;
    public ApplicationReadyEventHandler(RoleCommandService roleCommandService) { this.roleCommandService = roleCommandService; }
    @EventListener public void on(ApplicationReadyEvent event) {
        roleCommandService.handle(new SeedRolesCommand());
    }
}

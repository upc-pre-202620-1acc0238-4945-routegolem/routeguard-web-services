package com.routegolem.routeguard.iam.domain.services;
import com.routegolem.routeguard.iam.domain.model.entities.Role;
import com.routegolem.routeguard.iam.domain.model.queries.GetAllRolesQuery;
import com.routegolem.routeguard.iam.domain.model.queries.GetRoleByNameQuery;
import java.util.List;
import java.util.Optional;
public interface RoleQueryService {
    List<Role> handle(GetAllRolesQuery query);
    Optional<Role> handle(GetRoleByNameQuery query);
}

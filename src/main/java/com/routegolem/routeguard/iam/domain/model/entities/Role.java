package com.routegolem.routeguard.iam.domain.model.entities;
import com.routegolem.routeguard.iam.domain.model.valueobjects.Roles;
import lombok.Getter;
@Getter
public class Role {
    private Long id;
    private Roles name;
    public Role(Roles name) { this.name = name; }
    public Role(Long id, Roles name) { this.id = id; this.name = name; }
}

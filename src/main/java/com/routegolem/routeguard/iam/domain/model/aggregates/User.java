package com.routegolem.routeguard.iam.domain.model.aggregates;
import com.routegolem.routeguard.iam.domain.model.entities.Role;
import com.routegolem.routeguard.shared.domain.model.aggregates.AbstractDomainAggregateRoot;
import lombok.Getter;
import java.util.HashSet;
import java.util.Set;
@Getter
public class User extends AbstractDomainAggregateRoot<User> {
    private Long id;
    private String username;
    private String password;
    private Set<Role> roles;
    public User(String username, String password) {
        this.username = username;
        this.password = password;
        this.roles = new HashSet<>();
    }
    public User(String username, String password, Set<Role> roles) {
        this.username = username;
        this.password = password;
        this.roles = roles;
    }
    public User(Long id, String username, String password, Set<Role> roles) {
        this.id = id;
        this.username = username;
        this.password = password;
        this.roles = roles;
    }
    public User addRole(Role role) {
        this.roles.add(role);
        return this;
    }
}

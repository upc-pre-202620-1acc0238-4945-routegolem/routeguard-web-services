package com.routegolem.routeguard.iam.application.internal.commandservices;
import com.routegolem.routeguard.iam.domain.model.aggregates.User;
import com.routegolem.routeguard.iam.domain.model.commands.SignInCommand;
import com.routegolem.routeguard.iam.domain.model.commands.SignUpCommand;
import com.routegolem.routeguard.iam.domain.model.entities.Role;
import com.routegolem.routeguard.iam.domain.model.repositories.RoleRepository;
import com.routegolem.routeguard.iam.domain.model.repositories.UserRepository;
import com.routegolem.routeguard.iam.domain.model.valueobjects.Roles;
import com.routegolem.routeguard.iam.domain.services.UserCommandService;
import com.routegolem.routeguard.iam.infrastructure.hashing.bcrypt.BCryptHashingService;
import com.routegolem.routeguard.iam.infrastructure.tokens.jwt.BearerTokenService;
import org.apache.commons.lang3.tuple.ImmutablePair;
import org.springframework.stereotype.Service;
import java.util.Optional;
@Service
public class UserCommandServiceImpl implements UserCommandService {
    private final UserRepository userRepository;
    private final RoleRepository roleRepository;
    private final BCryptHashingService hashingService;
    private final BearerTokenService tokenService;
    public UserCommandServiceImpl(UserRepository userRepository, RoleRepository roleRepository, BCryptHashingService hashingService, BearerTokenService tokenService) {
        this.userRepository = userRepository;
        this.roleRepository = roleRepository;
        this.hashingService = hashingService;
        this.tokenService = tokenService;
    }
    @Override public Optional<User> handle(SignUpCommand command) {
        if (userRepository.existsByUsername(command.username())) throw new RuntimeException("Username is already taken");
        User user = new User(command.username(), hashingService.encode(command.password()));
        command.roles().forEach(r -> {
            Role role = roleRepository.findByName(Roles.valueOf(r)).orElseThrow(() -> new RuntimeException("Role not found"));
            user.addRole(role);
        });
        if (user.getRoles().isEmpty()) {
            Role role = roleRepository.findByName(Roles.ROLE_USER).orElseThrow(() -> new RuntimeException("Role not found"));
            user.addRole(role);
        }
        return Optional.of(userRepository.save(user));
    }
    @Override public Optional<ImmutablePair<User, String>> handle(SignInCommand command) {
        User user = userRepository.findByUsername(command.username()).orElseThrow(() -> new RuntimeException("User not found"));
        if (!hashingService.matches(command.password(), user.getPassword())) throw new RuntimeException("Invalid password");
        String token = tokenService.generateToken(user.getUsername());
        return Optional.of(new ImmutablePair<>(user, token));
    }
}

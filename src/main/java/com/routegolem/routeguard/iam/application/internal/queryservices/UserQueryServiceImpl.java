package com.routegolem.routeguard.iam.application.internal.queryservices;
import com.routegolem.routeguard.iam.domain.model.aggregates.User;
import com.routegolem.routeguard.iam.domain.model.queries.GetAllUsersQuery;
import com.routegolem.routeguard.iam.domain.model.queries.GetUserByIdQuery;
import com.routegolem.routeguard.iam.domain.model.queries.GetUserByUsernameQuery;
import com.routegolem.routeguard.iam.domain.model.repositories.UserRepository;
import com.routegolem.routeguard.iam.domain.services.UserQueryService;
import org.springframework.stereotype.Service;
import java.util.List;
import java.util.Optional;
@Service
public class UserQueryServiceImpl implements UserQueryService {
    private final UserRepository userRepository;
    public UserQueryServiceImpl(UserRepository userRepository) { this.userRepository = userRepository; }
    @Override public List<User> handle(GetAllUsersQuery query) { return userRepository.findAll(); }
    @Override public Optional<User> handle(GetUserByIdQuery query) { return userRepository.findById(query.userId()); }
    @Override public Optional<User> handle(GetUserByUsernameQuery query) { return userRepository.findByUsername(query.username()); }
}

package com.routegolem.routeguard.iam.domain.model.repositories;
import com.routegolem.routeguard.iam.domain.model.aggregates.User;
import java.util.Optional;
import java.util.List;
public interface UserRepository {
    Optional<User> findByUsername(String username);
    boolean existsByUsername(String username);
    Optional<User> findById(Long id);
    User save(User user);
    List<User> findAll();
}

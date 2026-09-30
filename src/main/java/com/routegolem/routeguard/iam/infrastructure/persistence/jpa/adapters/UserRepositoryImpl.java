package com.routegolem.routeguard.iam.infrastructure.persistence.jpa.adapters;
import com.routegolem.routeguard.iam.domain.model.aggregates.User;
import com.routegolem.routeguard.iam.domain.model.repositories.UserRepository;
import com.routegolem.routeguard.iam.infrastructure.persistence.jpa.assemblers.UserAssembler;
import com.routegolem.routeguard.iam.infrastructure.persistence.jpa.repositories.JpaUserRepository;
import org.springframework.stereotype.Repository;
import java.util.List;
import java.util.Optional;
import java.util.stream.Collectors;
@Repository
public class UserRepositoryImpl implements UserRepository {
    private final JpaUserRepository jpaUserRepository;
    public UserRepositoryImpl(JpaUserRepository jpaUserRepository) { this.jpaUserRepository = jpaUserRepository; }
    @Override public Optional<User> findByUsername(String username) { return jpaUserRepository.findByUsername(username).map(UserAssembler::toDomain); }
    @Override public boolean existsByUsername(String username) { return jpaUserRepository.existsByUsername(username); }
    @Override public Optional<User> findById(Long id) { return jpaUserRepository.findById(id).map(UserAssembler::toDomain); }
    @Override public User save(User user) { return UserAssembler.toDomain(jpaUserRepository.save(UserAssembler.toPersistence(user))); }
    @Override public List<User> findAll() { return jpaUserRepository.findAll().stream().map(UserAssembler::toDomain).collect(Collectors.toList()); }
}

package com.routegolem.routeguard.iam.infrastructure.tokens.jwt;
import org.springframework.security.core.Authentication;
import jakarta.servlet.http.HttpServletRequest;
public interface BearerTokenService {
    String generateToken(Authentication authentication);
    String generateToken(String username);
    String getUsernameFromToken(String token);
    boolean validateToken(String token);
    String resolveToken(HttpServletRequest request);
}

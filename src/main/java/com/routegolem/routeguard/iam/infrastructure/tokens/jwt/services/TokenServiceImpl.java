package com.routegolem.routeguard.iam.infrastructure.tokens.jwt.services;
import com.routegolem.routeguard.iam.infrastructure.tokens.jwt.BearerTokenService;
import io.jsonwebtoken.Jwts;
import io.jsonwebtoken.security.Keys;
import jakarta.servlet.http.HttpServletRequest;
import org.springframework.security.core.Authentication;
import org.springframework.stereotype.Service;
import java.util.Date;
import javax.crypto.SecretKey;
@Service
public class TokenServiceImpl implements BearerTokenService {
    private final String secret = "this-is-a-very-long-secret-key-for-jwt-signing";
    private final SecretKey key = Keys.hmacShaKeyFor(secret.getBytes());
    private final long expiration = 86400000;
    @Override public String generateToken(Authentication authentication) { return generateToken(authentication.getName()); }
    @Override public String generateToken(String username) {
        return Jwts.builder()
                .subject(username)
                .issuedAt(new Date())
                .expiration(new Date(System.currentTimeMillis() + expiration))
                .signWith(key)
                .compact();
    }
    @Override public String getUsernameFromToken(String token) {
        return Jwts.parser().verifyWith(key).build().parseSignedClaims(token).getPayload().getSubject();
    }
    @Override public boolean validateToken(String token) {
        try { Jwts.parser().verifyWith(key).build().parseSignedClaims(token); return true; } catch (Exception e) { return false; }
    }
    @Override public String resolveToken(HttpServletRequest request) {
        String bearerToken = request.getHeader("Authorization");
        if (bearerToken != null && bearerToken.startsWith("Bearer ")) return bearerToken.substring(7);
        return null;
    }
}

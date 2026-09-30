package com.routegolem.routeguard.shared.interfaces.rest.resources;

import com.routegolem.routeguard.shared.application.Result;
import org.springframework.http.ResponseEntity;

public class ResponseEntityAssembler {
    
    public static <T, E> ResponseEntity<?> toResponse(Result<T, E> result) {
        if (result.isSuccess()) {
            return ResponseEntity.ok(result.value());
        } else {
            // Very simple error handling for generic purposes
            return ResponseEntity.badRequest().body(result.error());
        }
    }
}

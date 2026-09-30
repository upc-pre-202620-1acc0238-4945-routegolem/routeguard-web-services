package com.routegolem.routeguard.shared.interfaces.rest.exceptions;

import com.routegolem.routeguard.shared.interfaces.rest.resources.ErrorResource;
import com.routegolem.routeguard.shared.interfaces.rest.resources.ErrorResponseAssembler;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.ControllerAdvice;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.ResponseStatus;

@ControllerAdvice
public class GlobalExceptionHandler {

    @ExceptionHandler(IllegalArgumentException.class)
    @ResponseStatus(HttpStatus.BAD_REQUEST)
    public ResponseEntity<ErrorResource> handleIllegalArgumentException(IllegalArgumentException e) {
        return ResponseEntity.badRequest().body(ErrorResponseAssembler.toResource(e.getMessage()));
    }

    @ExceptionHandler(RuntimeException.class)
    @ResponseStatus(HttpStatus.INTERNAL_SERVER_ERROR)
    public ResponseEntity<ErrorResource> handleRuntimeException(RuntimeException e) {
        return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                .body(ErrorResponseAssembler.toResource(e.getMessage()));
    }
}

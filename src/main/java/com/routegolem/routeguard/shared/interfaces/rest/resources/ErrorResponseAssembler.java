package com.routegolem.routeguard.shared.interfaces.rest.resources;

import com.routegolem.routeguard.shared.application.ApplicationError;

import java.util.List;

public class ErrorResponseAssembler {

    public static ErrorResource toResource(ApplicationError error) {
        return new ErrorResource(List.of(error.message()));
    }

    public static ErrorResource toResource(List<ApplicationError> errors) {
        return new ErrorResource(errors.stream().map(ApplicationError::message).toList());
    }

    public static ErrorResource toResource(String message) {
        return new ErrorResource(List.of(message));
    }
}

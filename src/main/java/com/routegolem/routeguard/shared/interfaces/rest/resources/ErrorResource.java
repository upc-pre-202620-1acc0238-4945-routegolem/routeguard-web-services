package com.routegolem.routeguard.shared.interfaces.rest.resources;

import com.fasterxml.jackson.annotation.JsonInclude;
import org.jspecify.annotations.Nullable;

import java.util.List;

@JsonInclude(JsonInclude.Include.NON_NULL)
public record ErrorResource(String code, String message, @Nullable String details) {
    public ErrorResource(String code, String message) {
        this(code, message, null);
    }
}

package com.routegolem.routeguard.shared.application;

public record Result<T, E>(T value, E error) {
    public boolean isSuccess() {
        return error == null;
    }

    public boolean isFailure() {
        return error != null;
    }

    public static <T, E> Result<T, E> success(T value) {
        return new Result<>(value, null);
    }

    public static <T, E> Result<T, E> failure(E error) {
        return new Result<>(null, error);
    }
}

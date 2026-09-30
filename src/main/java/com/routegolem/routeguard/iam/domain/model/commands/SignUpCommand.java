package com.routegolem.routeguard.iam.domain.model.commands;
import java.util.List;
public record SignUpCommand(String username, String password, List<String> roles) {}

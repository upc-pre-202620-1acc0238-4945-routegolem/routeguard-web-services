package com.routegolem.routeguard;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.data.jpa.repository.config.EnableJpaAuditing;

@SpringBootApplication
@EnableJpaAuditing
public class RouteguardWebServicesApplication {

    public static void main(String[] args) {
        SpringApplication.run(RouteguardWebServicesApplication.class, args);
    }

}

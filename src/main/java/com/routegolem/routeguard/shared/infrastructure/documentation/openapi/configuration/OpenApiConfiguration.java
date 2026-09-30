package com.routegolem.routeguard.shared.infrastructure.documentation.openapi.configuration;

import io.swagger.v3.oas.models.ExternalDocumentation;
import io.swagger.v3.oas.models.OpenAPI;
import io.swagger.v3.oas.models.info.Info;
import io.swagger.v3.oas.models.info.License;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

@Configuration
public class OpenApiConfiguration {
    @Bean
    public OpenAPI routeGuardOpenApi() {
        return new OpenAPI()
                .info(new Info().title("RouteGuard API")
                        .description("RouteGuard application REST API documentation.")
                        .version("v1.0.0")
                        .license(new License().name("Apache 2.0").url("https://springdoc.org")))
                .externalDocs(new ExternalDocumentation()
                        .description("RouteGuard Wiki Documentation")
                        .url("https://routeguard.wiki.github.org/docs"));
    }
}

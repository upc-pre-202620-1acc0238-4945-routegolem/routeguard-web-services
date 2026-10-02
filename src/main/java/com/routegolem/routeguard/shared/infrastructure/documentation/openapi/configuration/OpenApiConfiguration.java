package com.routegolem.routeguard.shared.infrastructure.documentation.openapi.configuration;

import io.swagger.v3.oas.models.ExternalDocumentation;
import io.swagger.v3.oas.models.OpenAPI;
import io.swagger.v3.oas.models.info.Contact;
import io.swagger.v3.oas.models.info.Info;
import io.swagger.v3.oas.models.info.License;
import io.swagger.v3.oas.models.servers.Server;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

import java.util.List;

@Configuration
public class OpenApiConfiguration {
    // Properties
    @Value("${spring.application.name}")
    String applicationName;

    @Value("${documentation.application.description}")
    String applicationDescription;

    @Value("${documentation.application.version}")
    String applicationVersion;

    //Methods
    @Bean
    public OpenAPI routeGuardOpenApi() {
        var openApi = new OpenAPI();
        openApi
                .info(new Info()
                        .title(this.applicationName)
                        .description(this.applicationDescription)
                        .version(this.applicationVersion)
                        .contact(new Contact()
                                .name("RouteGuard Application Support")
                                .email("support@routeguard.com")
                                .url("https://routeguard.com/support")
                        )
                        .license(new License()
                                .name("Apache 2.0")
                                .url("https://www.apache.org/licenses/LICENSE-2.0.html")
                        )
                ).externalDocs(new ExternalDocumentation()
                        .description("RouteGuard Wiki Documentation")
                        .url("https://routeguard.wiki.github.org/docs")
                );

        openApi.servers(List.of(
                new Server()
                        .url("http:localhost:8080")
                        .description("Local Development Environment"),
                new Server()
                        .url("https://staging-api.routeguard.com")
                        .description("Staging Environment"),
                new Server()
                        .url("https://api.routeguard.com")
                        .description("Production Environment")
        ));

        /*
        return new OpenAPI()
                .info(new Info().title("RouteGuard API")
                        .description("RouteGuard application REST API documentation.")
                        .version("v1.0.0")
                        .license(new License().name("Apache 2.0").url("https://springdoc.org")))
                .externalDocs(new ExternalDocumentation()
                        .description("RouteGuard Wiki Documentation")
                        .url("https://routeguard.wiki.github.org/docs"));
         */

        return openApi;
    }
}

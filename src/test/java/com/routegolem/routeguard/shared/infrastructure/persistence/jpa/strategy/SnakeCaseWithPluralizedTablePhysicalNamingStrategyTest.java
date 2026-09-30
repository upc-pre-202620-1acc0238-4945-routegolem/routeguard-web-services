package com.routegolem.routeguard.shared.infrastructure.persistence.jpa.strategy;

import org.hibernate.boot.model.naming.Identifier;
import org.junit.jupiter.api.Test;
import static org.junit.jupiter.api.Assertions.assertEquals;

public class SnakeCaseWithPluralizedTablePhysicalNamingStrategyTest {

    @Test
    public void testStrategy() {
        SnakeCaseWithPluralizedTablePhysicalNamingStrategy strategy = new SnakeCaseWithPluralizedTablePhysicalNamingStrategy();
        
        Identifier tableName = Identifier.toIdentifier("UserRole");
        assertEquals("user_roles", strategy.toPhysicalTableName(tableName, null).getText());
        
        Identifier columnName = Identifier.toIdentifier("createdAt");
        assertEquals("created_at", strategy.toPhysicalColumnName(columnName, null).getText());
        
        Identifier columnName2 = Identifier.toIdentifier("HTTPResponseCode");
        assertEquals("http_response_code", strategy.toPhysicalColumnName(columnName2, null).getText());
    }
}

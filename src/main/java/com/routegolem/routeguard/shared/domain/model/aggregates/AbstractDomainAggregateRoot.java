package com.routegolem.routeguard.shared.domain.model.aggregates;

import org.springframework.data.domain.AbstractAggregateRoot;

public abstract class AbstractDomainAggregateRoot<T extends AbstractAggregateRoot<T>> extends AbstractAggregateRoot<T> {

}

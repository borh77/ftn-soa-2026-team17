package com.soa.purchase.repository;

import com.soa.purchase.entity.ShoppingCart;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;

public interface ShoppingCartRepository extends JpaRepository<ShoppingCart, Long> {
    Optional<ShoppingCart> findByTouristId(Long touristId);
}

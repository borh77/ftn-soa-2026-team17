package com.soa.purchase.controller;

import com.soa.purchase.dto.CheckoutResponse;
import com.soa.purchase.dto.ShoppingCartResponse;
import com.soa.purchase.dto.TourPurchaseTokenResponse;
import com.soa.purchase.security.SecurityUtils;
import com.soa.purchase.service.PurchaseService;
import lombok.RequiredArgsConstructor;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.Map;

@RestController
@RequiredArgsConstructor
public class PurchaseController {
    private final PurchaseService purchaseService;

    @GetMapping("/api/purchase/ping")
    public String ping() {
        return "Purchase service is running";
    }

    @GetMapping("/api/cart")
    public ShoppingCartResponse getCart() {
        return purchaseService.getCart(SecurityUtils.currentPersonId());
    }

    @PostMapping("/api/cart/items/{tourId}")
    public ShoppingCartResponse addTourToCart(
            @PathVariable Long tourId) {
        return purchaseService.addTourToCart(SecurityUtils.currentPersonId(), tourId);
    }

    @PostMapping("/api/cart/items/{tourId}/simulate-saga-failure")
public ShoppingCartResponse simulateAddToCartSagaFailure(
        @PathVariable Long tourId) {
    return purchaseService.simulateAddToCartSagaFailure(
            SecurityUtils.currentPersonId(),
            tourId
    );
}

    @DeleteMapping("/api/cart/items/{itemId}")
    public ShoppingCartResponse removeItem(@PathVariable Long itemId) {
        return purchaseService.removeItem(SecurityUtils.currentPersonId(), itemId);
    }

    @PostMapping("/api/cart/checkout")
    public CheckoutResponse checkout() {
        return purchaseService.checkout(SecurityUtils.currentPersonId());
    }

    @GetMapping("/api/purchases/tokens")
    public List<TourPurchaseTokenResponse> getMyTokens() {
        return purchaseService.getMyTokens(SecurityUtils.currentPersonId());
    }

    @GetMapping("/api/purchases/tours/{tourId}/purchased")
    public Map<String, Boolean> hasPurchased(@PathVariable Long tourId) {
        return Map.of("purchased", purchaseService.hasPurchased(SecurityUtils.currentPersonId(), tourId));
    }
}

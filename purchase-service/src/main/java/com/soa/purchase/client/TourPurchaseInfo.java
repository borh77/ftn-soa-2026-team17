package com.soa.purchase.client;

import java.math.BigDecimal;

public record TourPurchaseInfo(Long id, String name, BigDecimal price, String status) {
    public boolean canBePurchased() {
        return "Published".equalsIgnoreCase(status);
    }
}

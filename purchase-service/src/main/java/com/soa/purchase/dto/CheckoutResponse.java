package com.soa.purchase.dto;

import java.util.List;

public record CheckoutResponse(List<TourPurchaseTokenResponse> tokens) {}

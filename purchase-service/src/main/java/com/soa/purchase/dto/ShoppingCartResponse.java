package com.soa.purchase.dto;

import java.math.BigDecimal;
import java.util.List;

public record ShoppingCartResponse(Long id, Long touristId, List<OrderItemResponse> items, BigDecimal totalPrice) {}

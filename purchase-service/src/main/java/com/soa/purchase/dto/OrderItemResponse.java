package com.soa.purchase.dto;

import java.math.BigDecimal;

public record OrderItemResponse(Long id, Long tourId, String tourName, BigDecimal price) {}

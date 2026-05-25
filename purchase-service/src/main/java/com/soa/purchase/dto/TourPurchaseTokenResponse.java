package com.soa.purchase.dto;

import java.time.LocalDateTime;

public record TourPurchaseTokenResponse(Long id, Long touristId, Long tourId, String tourName, String token, LocalDateTime createdAt) {}

package com.soa.purchase.repository;

import com.soa.purchase.entity.TourPurchaseToken;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.Optional;

public interface TourPurchaseTokenRepository extends JpaRepository<TourPurchaseToken, Long> {
    List<TourPurchaseToken> findByTouristId(Long touristId);
    boolean existsByTouristIdAndTourId(Long touristId, Long tourId);
    Optional<TourPurchaseToken> findByTouristIdAndTourId(Long touristId, Long tourId);
}

package com.soa.purchase.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

import java.time.LocalDateTime;
import java.util.UUID;

@Entity
@Getter
@Setter
@NoArgsConstructor
@Table(uniqueConstraints = @UniqueConstraint(columnNames = {"touristId", "tourId"}))
public class TourPurchaseToken {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(nullable = false)
    private Long touristId;

    @Column(nullable = false)
    private Long tourId;

    @Column(nullable = false)
    private String tourName;

    @Column(nullable = false, unique = true)
    private String token;

    @Column(nullable = false)
    private LocalDateTime createdAt;

    public TourPurchaseToken(Long touristId, Long tourId, String tourName) {
        this.touristId = touristId;
        this.tourId = tourId;
        this.tourName = tourName;
        this.token = UUID.randomUUID().toString();
        this.createdAt = LocalDateTime.now();
    }
}

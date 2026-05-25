package com.soa.purchase.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

import java.math.BigDecimal;
import java.util.ArrayList;
import java.util.List;

@Entity
@Getter
@Setter
@NoArgsConstructor
public class ShoppingCart {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(nullable = false, unique = true)
    private Long touristId;

    @Column(nullable = false, precision = 12, scale = 2)
    private BigDecimal totalPrice = BigDecimal.ZERO;

    @OneToMany(mappedBy = "cart", cascade = CascadeType.ALL, orphanRemoval = true, fetch = FetchType.EAGER)
    private List<OrderItem> items = new ArrayList<>();

    public ShoppingCart(Long touristId) {
        this.touristId = touristId;
    }

    public void addItem(OrderItem item) {
        boolean alreadyExists = items.stream().anyMatch(i -> i.getTourId().equals(item.getTourId()));
        if (alreadyExists) {
            throw new IllegalArgumentException("Tura je već u korpi.");
        }
        item.setCart(this);
        items.add(item);
        recalculateTotalPrice();
    }

    public void removeItem(Long itemId) {
        boolean removed = items.removeIf(item -> item.getId().equals(itemId));
        if (!removed) {
            throw new IllegalArgumentException("Stavka nije pronađena u korpi.");
        }
        recalculateTotalPrice();
    }

    public void clear() {
        items.clear();
        recalculateTotalPrice();
    }

    public void recalculateTotalPrice() {
        totalPrice = items.stream()
                .map(OrderItem::getPrice)
                .reduce(BigDecimal.ZERO, BigDecimal::add);
    }
}

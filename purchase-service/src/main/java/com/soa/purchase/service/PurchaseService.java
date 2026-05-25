package com.soa.purchase.service;

import com.soa.purchase.client.TourClient;
import com.soa.purchase.client.TourPurchaseInfo;
import com.soa.purchase.dto.*;
import com.soa.purchase.entity.OrderItem;
import com.soa.purchase.entity.ShoppingCart;
import com.soa.purchase.entity.TourPurchaseToken;
import com.soa.purchase.exception.BadRequestException;
import com.soa.purchase.repository.ShoppingCartRepository;
import com.soa.purchase.repository.TourPurchaseTokenRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;

@Service
@RequiredArgsConstructor
public class PurchaseService {
    private final ShoppingCartRepository cartRepository;
    private final TourPurchaseTokenRepository tokenRepository;
    private final TourClient tourClient;

    @Transactional
    public ShoppingCartResponse getCart(Long touristId) {
        ShoppingCart cart = getOrCreateCart(touristId);
        return toCartResponse(cart);
    }

    @Transactional
    public ShoppingCartResponse addTourToCart(Long touristId, Long tourId) {
        if (tokenRepository.existsByTouristIdAndTourId(touristId, tourId)) {
            throw new BadRequestException("Tura je već kupljena.");
        }

        TourPurchaseInfo tour = tourClient.getPurchaseInfo(tourId);
        if (tour == null) {
            throw new BadRequestException("Tura nije pronađena.");
        }
        if (!tour.canBePurchased()) {
            throw new BadRequestException("Samo objavljena tura može da se kupi.");
        }

        ShoppingCart cart = getOrCreateCart(touristId);
        cart.addItem(new OrderItem(tour.id(), tour.name(), tour.price()));
        return toCartResponse(cartRepository.save(cart));
    }

    @Transactional
    public ShoppingCartResponse removeItem(Long touristId, Long itemId) {
        ShoppingCart cart = getOrCreateCart(touristId);
        cart.removeItem(itemId);
        return toCartResponse(cartRepository.save(cart));
    }

    @Transactional
    public CheckoutResponse checkout(Long touristId) {
        ShoppingCart cart = getOrCreateCart(touristId);
        if (cart.getItems().isEmpty()) {
            throw new BadRequestException("Korpa je prazna.");
        }

        List<TourPurchaseToken> tokens = cart.getItems().stream()
                .filter(item -> !tokenRepository.existsByTouristIdAndTourId(touristId, item.getTourId()))
                .map(item -> new TourPurchaseToken(touristId, item.getTourId(), item.getTourName()))
                .toList();

        List<TourPurchaseToken> savedTokens = tokenRepository.saveAll(tokens);
        cart.clear();
        cartRepository.save(cart);

        return new CheckoutResponse(savedTokens.stream().map(this::toTokenResponse).toList());
    }

    @Transactional(readOnly = true)
    public List<TourPurchaseTokenResponse> getMyTokens(Long touristId) {
        return tokenRepository.findByTouristId(touristId).stream().map(this::toTokenResponse).toList();
    }

    @Transactional(readOnly = true)
    public boolean hasPurchased(Long touristId, Long tourId) {
        return tokenRepository.existsByTouristIdAndTourId(touristId, tourId);
    }

    private ShoppingCart getOrCreateCart(Long touristId) {
        return cartRepository.findByTouristId(touristId).orElseGet(() -> cartRepository.save(new ShoppingCart(touristId)));
    }

    private ShoppingCartResponse toCartResponse(ShoppingCart cart) {
        return new ShoppingCartResponse(
                cart.getId(),
                cart.getTouristId(),
                cart.getItems().stream().map(this::toItemResponse).toList(),
                cart.getTotalPrice()
        );
    }

    private OrderItemResponse toItemResponse(OrderItem item) {
        return new OrderItemResponse(item.getId(), item.getTourId(), item.getTourName(), item.getPrice());
    }

    private TourPurchaseTokenResponse toTokenResponse(TourPurchaseToken token) {
        return new TourPurchaseTokenResponse(
                token.getId(),
                token.getTouristId(),
                token.getTourId(),
                token.getTourName(),
                token.getToken(),
                token.getCreatedAt()
        );
    }
}

package com.soa.purchase.grpc;

import com.soa.purchase.dto.OrderItemResponse;
import com.soa.purchase.dto.ShoppingCartResponse;
import com.soa.purchase.dto.TourPurchaseTokenResponse;
import com.soa.purchase.service.PurchaseService;
import io.grpc.stub.StreamObserver;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

@Component
@RequiredArgsConstructor
public class PurchaseGrpcController extends PurchaseGrpcServiceGrpc.PurchaseGrpcServiceImplBase {

    private final PurchaseService purchaseService;

    @Override
    public void getCart(PurchaseUserRequest request, StreamObserver<com.soa.purchase.grpc.ShoppingCartResponse> responseObserver) {
        ShoppingCartResponse cart = purchaseService.getCart(request.getTouristId());

        var responseBuilder = com.soa.purchase.grpc.ShoppingCartResponse.newBuilder()
                .setId(cart.id())
                .setTouristId(cart.touristId())
                .setTotalPrice(cart.totalPrice().doubleValue());

        for (OrderItemResponse item : cart.items()) {
            responseBuilder.addItems(
                    com.soa.purchase.grpc.OrderItemResponse.newBuilder()
                            .setId(item.id())
                            .setTourId(item.tourId())
                            .setTourName(item.tourName())
                            .setPrice(item.price().doubleValue())
                            .build()
            );
        }

        responseObserver.onNext(responseBuilder.build());
        responseObserver.onCompleted();
    }

    @Override
    public void checkoutCart(PurchaseUserRequest request, StreamObserver<com.soa.purchase.grpc.CheckoutResponse> responseObserver) {
        var checkout = purchaseService.checkout(request.getTouristId());

        var responseBuilder = com.soa.purchase.grpc.CheckoutResponse.newBuilder();

        for (TourPurchaseTokenResponse token : checkout.tokens()) {
            responseBuilder.addTokens(
                    com.soa.purchase.grpc.TourPurchaseTokenResponse.newBuilder()
                            .setId(token.id())
                            .setTouristId(token.touristId())
                            .setTourId(token.tourId())
                            .setTourName(token.tourName())
                            .setToken(token.token())
                            .setCreatedAt(token.createdAt().toString())
                            .build()
            );
        }

        responseObserver.onNext(responseBuilder.build());
        responseObserver.onCompleted();
    }
}
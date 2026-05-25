package com.soa.purchase.grpc;

import io.grpc.Server;
import io.grpc.ServerBuilder;
import jakarta.annotation.PostConstruct;
import jakarta.annotation.PreDestroy;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

@Component
@RequiredArgsConstructor
public class PurchaseGrpcServer {

    private Server server;

    private final PurchaseGrpcController purchaseGrpcController;

    @PostConstruct
    public void start() throws Exception {
        server = ServerBuilder
                .forPort(9091)
                .addService(purchaseGrpcController)
                .build()
                .start();

        System.out.println("Purchase gRPC server started on port 9091");
    }

    @PreDestroy
    public void stop() {
        if (server != null) {
            server.shutdown();
        }
    }
}
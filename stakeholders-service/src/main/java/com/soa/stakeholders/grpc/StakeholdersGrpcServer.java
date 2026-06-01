package com.soa.stakeholders.grpc;

import com.soa.stakeholders.service.AdminService;
import com.soa.stakeholders.service.UserService;
import io.grpc.Server;
import io.grpc.ServerBuilder;
import jakarta.annotation.PostConstruct;
import jakarta.annotation.PreDestroy;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

import java.io.IOException;

@Component
@RequiredArgsConstructor
public class StakeholdersGrpcServer {

    private static final int GRPC_PORT = 9092;

    private final UserService userService;
    private final AdminService adminService;

    private Server server;

    @PostConstruct
    public void start() throws IOException {
        server = ServerBuilder
                .forPort(GRPC_PORT)
                .addService(new StakeholdersGrpcController(userService, adminService))
                .build()
                .start();

        System.out.println("Stakeholders gRPC server started on port " + GRPC_PORT);
    }

    @PreDestroy
    public void stop() {
        if (server != null) {
            server.shutdown();
            System.out.println("Stakeholders gRPC server stopped");
        }
    }
}
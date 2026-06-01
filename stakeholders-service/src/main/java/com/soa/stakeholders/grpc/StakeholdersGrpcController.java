package com.soa.stakeholders.grpc;

import com.soa.stakeholders.service.AdminService;
import com.soa.stakeholders.service.UserService;
import io.grpc.stub.StreamObserver;
import lombok.RequiredArgsConstructor;

@RequiredArgsConstructor
public class StakeholdersGrpcController
        extends StakeholdersGrpcServiceGrpc.StakeholdersGrpcServiceImplBase {

    private final UserService userService;
    private final AdminService adminService;

    @Override
    public void getMyProfile(
            GetMyProfileRequest request,
            StreamObserver<ProfileResponse> responseObserver) {

        com.soa.stakeholders.dto.ProfileResponse profile =
                userService.getMyProfile(request.getUsername());

        ProfileResponse response = ProfileResponse.newBuilder()
                .setUsername(nullToEmpty(profile.getUsername()))
                .setEmail(nullToEmpty(profile.getEmail()))
                .setRole(profile.getRole() == null ? "" : profile.getRole().name())
                .setFirstName(nullToEmpty(profile.getFirstName()))
                .setLastName(nullToEmpty(profile.getLastName()))
                .setProfileImage(nullToEmpty(profile.getProfileImage()))
                .setBiography(nullToEmpty(profile.getBiography()))
                .setMotto(nullToEmpty(profile.getMotto()))
                .build();

        responseObserver.onNext(response);
        responseObserver.onCompleted();
    }

    @Override
    public void getAllUsers(
            GetAllUsersRequest request,
            StreamObserver<GetAllUsersResponse> responseObserver) {

        GetAllUsersResponse.Builder responseBuilder =
                GetAllUsersResponse.newBuilder();

        for (com.soa.stakeholders.dto.UserResponse user : adminService.getAllUsers()) {
            com.soa.stakeholders.grpc.UserResponse grpcUser =
                    com.soa.stakeholders.grpc.UserResponse.newBuilder()
                            .setId(user.getId())
                            .setUsername(nullToEmpty(user.getUsername()))
                            .setEmail(nullToEmpty(user.getEmail()))
                            .setRole(user.getRole() == null ? "" : user.getRole().name())
                            .setBlocked(user.isBlocked())
                            .build();

            responseBuilder.addUsers(grpcUser);
        }

        responseObserver.onNext(responseBuilder.build());
        responseObserver.onCompleted();
    }

    private String nullToEmpty(String value) {
        return value == null ? "" : value;
    }
}
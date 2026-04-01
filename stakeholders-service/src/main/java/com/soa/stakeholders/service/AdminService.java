package com.soa.stakeholders.service;

import com.soa.stakeholders.dto.UserResponse;
import com.soa.stakeholders.entity.AppUser;
import com.soa.stakeholders.repository.AppUserRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
@RequiredArgsConstructor
public class AdminService {

    private final AppUserRepository appUserRepository;

    public List<UserResponse> getAllUsers() {
        return appUserRepository.findAll()
                .stream()
                .map(this::mapToUserResponse)
                .toList();
    }

    private UserResponse mapToUserResponse(AppUser user) {
        return new UserResponse(
                user.getId(),
                user.getUsername(),
                user.getEmail(),
                user.getRole(),
                user.isBlocked()
        );
    }
}
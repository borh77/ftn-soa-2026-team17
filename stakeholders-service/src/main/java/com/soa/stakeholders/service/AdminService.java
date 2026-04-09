package com.soa.stakeholders.service;

import com.soa.stakeholders.dto.UserResponse;
import com.soa.stakeholders.entity.AppUser;
import com.soa.stakeholders.entity.UserRole;
import com.soa.stakeholders.exception.BadRequestException;
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

    public UserResponse blockUser(Long id) {
        AppUser user = appUserRepository.findById(id)
                .orElseThrow(() -> new BadRequestException("User not found"));

        if (user.getRole() == UserRole.ADMIN) {
            throw new BadRequestException("Admin account cannot be blocked");
        }

        if (user.isBlocked()) {
            throw new BadRequestException("User is already blocked");
        }

        user.setBlocked(true);
        AppUser savedUser = appUserRepository.save(user);

        return mapToUserResponse(savedUser);
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
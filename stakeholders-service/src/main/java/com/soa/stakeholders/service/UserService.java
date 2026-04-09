package com.soa.stakeholders.service;

import com.soa.stakeholders.dto.ProfileResponse;
import com.soa.stakeholders.entity.AppUser;
import com.soa.stakeholders.exception.BadRequestException;
import com.soa.stakeholders.repository.AppUserRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

@Service
@RequiredArgsConstructor
public class UserService {

    private final AppUserRepository appUserRepository;

    public ProfileResponse getMyProfile(String username) {
        AppUser user = appUserRepository.findByUsername(username)
                .orElseThrow(() -> new BadRequestException("User not found"));

        return new ProfileResponse(
                user.getUsername(),
                user.getEmail(),
                user.getRole(),
                user.getFirstName(),
                user.getLastName(),
                user.getProfileImage(),
                user.getBiography(),
                user.getMotto()
        );
    }
}
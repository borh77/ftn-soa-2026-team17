package com.soa.stakeholders.service;

import com.soa.stakeholders.dto.ProfileResponse;
import com.soa.stakeholders.entity.AppUser;
import com.soa.stakeholders.exception.BadRequestException;
import com.soa.stakeholders.repository.AppUserRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import com.soa.stakeholders.dto.UpdateProfileRequest;
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


    public ProfileResponse updateMyProfile(String username, UpdateProfileRequest request) {
        AppUser user = appUserRepository.findByUsername(username)
                .orElseThrow(() -> new BadRequestException("User not found"));

        if (request.getFirstName() != null) user.setFirstName(request.getFirstName());
        if (request.getLastName() != null) user.setLastName(request.getLastName());
        if (request.getProfileImage() != null) user.setProfileImage(request.getProfileImage());
        if (request.getBiography() != null) user.setBiography(request.getBiography());
        if (request.getMotto() != null) user.setMotto(request.getMotto());

        appUserRepository.save(user);

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
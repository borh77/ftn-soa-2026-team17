package com.soa.stakeholders.service;

import com.soa.stakeholders.dto.RegisterRequest;
import com.soa.stakeholders.dto.UserResponse;
import com.soa.stakeholders.entity.AppUser;
import com.soa.stakeholders.entity.UserRole;
import com.soa.stakeholders.exception.BadRequestException;
import com.soa.stakeholders.repository.AppUserRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;
import com.soa.stakeholders.dto.LoginRequest;
import com.soa.stakeholders.security.JwtService;

@Service
@RequiredArgsConstructor
public class AuthService {

    private final AppUserRepository appUserRepository;
    private final PasswordEncoder passwordEncoder;
    private final JwtService jwtService;

    public UserResponse register(RegisterRequest request) {
        if (request.getRole() == UserRole.ADMIN) {
            throw new BadRequestException("Admin cannot be registered through this endpoint");
        }

        if (appUserRepository.existsByUsername(request.getUsername())) {
            throw new BadRequestException("Username already exists");
        }

        if (appUserRepository.existsByEmail(request.getEmail())) {
            throw new BadRequestException("Email already exists");
        }

        if (request.getRole() != UserRole.GUIDE && request.getRole() != UserRole.TOURIST) {
            throw new BadRequestException("Invalid role");
        }

        AppUser appUser = AppUser.builder()
                .username(request.getUsername())
                .email(request.getEmail())
                .password(passwordEncoder.encode(request.getPassword()))
                .role(request.getRole())
                .isBlocked(false)
                .build();

        AppUser savedUser = appUserRepository.save(appUser);

        return new UserResponse(
                savedUser.getId(),
                savedUser.getUsername(),
                savedUser.getEmail(),
                savedUser.getRole(),
                savedUser.isBlocked()
        );
    }

    public String login(LoginRequest request) {
        AppUser user = appUserRepository.findByUsername(request.getUsername())
                .orElseThrow(() -> new BadRequestException("Invalid username or password"));

        if (!passwordEncoder.matches(request.getPassword(), user.getPassword())) {
            throw new BadRequestException("Invalid username or password");
        }

        if (user.isBlocked()) {
            throw new BadRequestException("User is blocked");
        }

        return jwtService.generateToken(user.getUsername());
    }
}
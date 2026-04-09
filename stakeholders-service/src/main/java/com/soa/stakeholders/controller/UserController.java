package com.soa.stakeholders.controller;

import com.soa.stakeholders.dto.ProfileResponse;
import com.soa.stakeholders.service.UserService;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/users")
@RequiredArgsConstructor
public class UserController {

    private final UserService userService;

    @GetMapping("/me")
    public ResponseEntity<ProfileResponse> getMyProfile(Authentication authentication) {
        String username = authentication.getName();
        return ResponseEntity.ok(userService.getMyProfile(username));
    }
}
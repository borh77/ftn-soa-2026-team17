package com.soa.stakeholders.dto;

import com.soa.stakeholders.entity.UserRole;
import lombok.AllArgsConstructor;
import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
@AllArgsConstructor
public class UserResponse {

    private Long id;
    private String username;
    private String email;
    private UserRole role;
    private boolean isBlocked;
}
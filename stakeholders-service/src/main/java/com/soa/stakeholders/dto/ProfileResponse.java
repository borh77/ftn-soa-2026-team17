package com.soa.stakeholders.dto;

import com.soa.stakeholders.entity.UserRole;
import lombok.AllArgsConstructor;
import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
@AllArgsConstructor
public class ProfileResponse {

    private String username;
    private String email;
    private UserRole role;

    private String firstName;
    private String lastName;
    private String profileImage;
    private String biography;
    private String motto;
}
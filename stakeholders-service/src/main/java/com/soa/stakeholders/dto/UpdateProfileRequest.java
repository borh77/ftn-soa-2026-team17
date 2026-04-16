package com.soa.stakeholders.dto;

import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class UpdateProfileRequest {
    private String firstName;
    private String lastName;
    private String profileImage;
    private String biography;
    private String motto;
}
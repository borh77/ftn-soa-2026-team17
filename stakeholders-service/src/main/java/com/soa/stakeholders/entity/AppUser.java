package com.soa.stakeholders.entity;

import jakarta.persistence.*;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity
@Table(name = "users")
@Getter
@Setter
@NoArgsConstructor
@AllArgsConstructor
@Builder
public class AppUser {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(nullable = false, unique = true)
    private String username;

    @Column(nullable = false, unique = true)
    private String email;

    @Column(nullable = false)
    private String password;

    @Enumerated(EnumType.STRING)
    @Column(nullable = false)
    private UserRole role;

    @Builder.Default
    @Column(nullable = false)
    private boolean isBlocked = false;

    @Column(nullable = true) 
    private String firstName;

    @Column(nullable = true)
    private String lastName;

    @Column(columnDefinition = "TEXT", nullable = true)
    private String profileImage;

    @Column(length = 2000, nullable = true)
    private String biography;

    @Column(nullable = true)
    private String motto;
}

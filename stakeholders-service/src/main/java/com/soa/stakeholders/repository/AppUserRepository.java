package com.soa.stakeholders.repository;

import com.soa.stakeholders.entity.AppUser;

import java.util.Optional;

import org.springframework.data.jpa.repository.JpaRepository;

public interface AppUserRepository extends JpaRepository<AppUser, Long> {

    boolean existsByUsername(String username);

    boolean existsByEmail(String email);

    Optional<AppUser> findByUsername(String username);
}
package com.soa.purchase.security;

public record AuthenticatedUser(Long personId, String username, String role) {}

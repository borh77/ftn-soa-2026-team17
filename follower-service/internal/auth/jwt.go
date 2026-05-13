package auth

import (
	"errors"
	"net/http"
	"os"
	"strings"

	"github.com/golang-jwt/jwt/v5"
)

func GetPersonIdFromRequest(r *http.Request) (int64, error) {
	authHeader := r.Header.Get("Authorization")

	if authHeader == "" {
		return 0, errors.New("authorization header is missing")
	}

	tokenString := authHeader

	if strings.HasPrefix(authHeader, "Bearer ") {
		tokenString = strings.TrimPrefix(authHeader, "Bearer ")
	}

	secret := os.Getenv("JWT_SECRET")

	if secret == "" {
		return 0, errors.New("jwt secret is missing")
	}

	token, err := jwt.Parse(tokenString, func(token *jwt.Token) (interface{}, error) {
		return []byte(secret), nil
	})

	if err != nil || !token.Valid {
		return 0, errors.New("invalid token")
	}

	claims, ok := token.Claims.(jwt.MapClaims)

	if !ok {
		return 0, errors.New("invalid token claims")
	}

	personIdValue, ok := claims["personId"]

	if !ok {
		return 0, errors.New("personId claim is missing")
	}

	personIdFloat, ok := personIdValue.(float64)

	if !ok {
		return 0, errors.New("personId claim is not valid")
	}

	return int64(personIdFloat), nil
}
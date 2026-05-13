package main

import (
	"context"
	"log"
	"net/http"

	"ftn-soa-2026-team17/follower-service/internal/config"

	"github.com/gin-gonic/gin"
	"github.com/neo4j/neo4j-go-driver/v5/neo4j"
	swaggerFiles "github.com/swaggo/files"
	ginSwagger "github.com/swaggo/gin-swagger"

	"ftn-soa-2026-team17/follower-service/internal/handler"
	"ftn-soa-2026-team17/follower-service/internal/repository"
	_ "ftn-soa-2026-team17/follower-service/docs"
)

func main() {
	driver := config.NewNeo4jDriver()
	defer driver.Close(context.Background())

	followerRepository := repository.NewFollowerRepository(driver)
	followerHandler := handler.NewFollowerHandler(followerRepository)

	router := gin.Default()

	router.GET("/api/followers/health", func(c *gin.Context) {
		c.JSON(http.StatusOK, gin.H{
			"status":  "ok",
			"service": "follower-service",
		})
	})

	router.GET("/api/followers/test-db", func(c *gin.Context) {
		session := driver.NewSession(context.Background(), neo4j.SessionConfig{})
		defer session.Close(context.Background())

		c.JSON(http.StatusOK, gin.H{
			"database": "connected",
		})
	})

	router.GET("/swagger/*any", ginSwagger.WrapHandler(swaggerFiles.Handler))

	router.POST("/api/followers/follow", followerHandler.FollowUser)
	router.GET("/api/followers/:userId/recommendations", followerHandler.GetRecommendations)
	router.DELETE("/api/followers/unfollow", followerHandler.UnfollowUser)
	router.GET("/api/followers/can-comment", followerHandler.CanComment)


	log.Println("Follower service started on port 8082")

	router.Run(":8082")
}
package handler

import (
	"net/http"
	"strconv"

	"ftn-soa-2026-team17/follower-service/internal/model"
	"ftn-soa-2026-team17/follower-service/internal/repository"
	"ftn-soa-2026-team17/follower-service/internal/auth"

	"github.com/gin-gonic/gin"
)

type FollowerHandler struct {
	Repository *repository.FollowerRepository
}

func NewFollowerHandler(repository *repository.FollowerRepository) *FollowerHandler {
	return &FollowerHandler{Repository: repository}
}

// FollowUser godoc
// @Summary Follow user
// @Description Creates FOLLOWS relationship between authenticated user and selected user
// @Tags followers
// @Accept json
// @Produce json
// @Security BearerAuth
// @Param request body model.FollowRequest true "Follow request"
// @Success 200 {object} map[string]string
// @Failure 400 {object} map[string]string
// @Failure 401 {object} map[string]string
// @Failure 500 {object} map[string]string
// @Router /api/followers/follow [post]
func (h *FollowerHandler) FollowUser(c *gin.Context) {
	followerId, err := auth.GetPersonIdFromRequest(c.Request)
	if err != nil {
		c.JSON(http.StatusUnauthorized, gin.H{"error": err.Error()})
		return
	}

	var request model.FollowRequest

	if err := c.ShouldBindJSON(&request); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	err = h.Repository.FollowUser(followerId, request.FollowingId)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": err.Error()})
		return
	}

	c.JSON(http.StatusOK, gin.H{"message": "follow created"})
}

// GetRecommendations godoc
// @Summary Get profile recommendations
// @Description Returns users followed by the people that authenticated user already follows
// @Tags followers
// @Produce json
// @Security BearerAuth
// @Success 200 {object} map[string][]int64
// @Failure 401 {object} map[string]string
// @Failure 500 {object} map[string]string
// @Router /api/followers/recommendations [get]
func (h *FollowerHandler) GetRecommendations(c *gin.Context) {
	userId, err := auth.GetPersonIdFromRequest(c.Request)
	if err != nil {
		c.JSON(http.StatusUnauthorized, gin.H{"error": err.Error()})
		return
	}

	recommendations, err := h.Repository.GetRecommendations(userId)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": err.Error()})
		return
	}

	c.JSON(http.StatusOK, gin.H{"recommendations": recommendations})
}

// UnfollowUser godoc
// @Summary Unfollow user
// @Description Deletes FOLLOWS relationship between authenticated user and selected user
// @Tags followers
// @Accept json
// @Produce json
// @Security BearerAuth
// @Param request body model.FollowRequest true "Unfollow request"
// @Success 200 {object} map[string]string
// @Failure 400 {object} map[string]string
// @Failure 401 {object} map[string]string
// @Failure 500 {object} map[string]string
// @Router /api/followers/unfollow [delete]
func (h *FollowerHandler) UnfollowUser(c *gin.Context) {
	followerId, err := auth.GetPersonIdFromRequest(c.Request)
	if err != nil {
		c.JSON(http.StatusUnauthorized, gin.H{"error": err.Error()})
		return
	}

	var request model.FollowRequest

	if err := c.ShouldBindJSON(&request); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	err = h.Repository.UnfollowUser(followerId, request.FollowingId)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": err.Error()})
		return
	}

	c.JSON(http.StatusOK, gin.H{"message": "follow deleted"})
}

// CanComment godoc
// @Summary Check if authenticated user can comment on author's blog
// @Description User can comment only if they follow the blog author
// @Tags followers
// @Produce json
// @Security BearerAuth
// @Param authorId query int true "Blog author user ID"
// @Success 200 {object} map[string]bool
// @Failure 400 {object} map[string]string
// @Failure 401 {object} map[string]string
// @Failure 500 {object} map[string]string
// @Router /api/followers/can-comment [get]
func (h *FollowerHandler) CanComment(c *gin.Context) {
	followerId, err := auth.GetPersonIdFromRequest(c.Request)
	if err != nil {
		c.JSON(http.StatusUnauthorized, gin.H{"error": err.Error()})
		return
	}

	authorId, err := strconv.ParseInt(c.Query("authorId"), 10, 64)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "invalid author id"})
		return
	}

	canComment, err := h.Repository.IsFollowing(followerId, authorId)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": err.Error()})
		return
	}

	c.JSON(http.StatusOK, gin.H{"canComment": canComment})
}
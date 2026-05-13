package handler

import (
	"net/http"
	"strconv"

	"ftn-soa-2026-team17/follower-service/internal/model"
	"ftn-soa-2026-team17/follower-service/internal/repository"

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
// @Description Creates FOLLOWS relationship between two users
// @Tags followers
// @Accept json
// @Produce json
// @Param request body model.FollowRequest true "Follow request"
// @Success 200 {object} map[string]string
// @Failure 400 {object} map[string]string
// @Failure 500 {object} map[string]string
// @Router /api/followers/follow [post]
func (h *FollowerHandler) FollowUser(c *gin.Context) {
	var request model.FollowRequest

	if err := c.ShouldBindJSON(&request); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	err := h.Repository.FollowUser(request.FollowerId, request.FollowingId)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": err.Error()})
		return
	}

	c.JSON(http.StatusOK, gin.H{"message": "follow created"})
}

// GetRecommendations godoc
// @Summary Get profile recommendations
// @Description Returns users followed by the people that selected user already follows
// @Tags followers
// @Produce json
// @Param userId path int true "User ID"
// @Success 200 {object} map[string][]int64
// @Failure 400 {object} map[string]string
// @Failure 500 {object} map[string]string
// @Router /api/followers/{userId}/recommendations [get]
func (h *FollowerHandler) GetRecommendations(c *gin.Context) {
	userId, err := strconv.ParseInt(c.Param("userId"), 10, 64)

	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "invalid user id"})
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
// @Description Deletes FOLLOWS relationship between two users
// @Tags followers
// @Accept json
// @Produce json
// @Param request body model.FollowRequest true "Unfollow request"
// @Success 200 {object} map[string]string
// @Failure 400 {object} map[string]string
// @Failure 500 {object} map[string]string
// @Router /api/followers/unfollow [delete]
func (h *FollowerHandler) UnfollowUser(c *gin.Context) {
	var request model.FollowRequest

	if err := c.ShouldBindJSON(&request); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	err := h.Repository.UnfollowUser(request.FollowerId, request.FollowingId)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": err.Error()})
		return
	}

	c.JSON(http.StatusOK, gin.H{"message": "follow deleted"})
}

// CanComment godoc
// @Summary Check if user can comment on author's blog
// @Description User can comment only if they follow the blog author
// @Tags followers
// @Produce json
// @Param followerId query int true "Follower user ID"
// @Param authorId query int true "Blog author user ID"
// @Success 200 {object} map[string]bool
// @Failure 400 {object} map[string]string
// @Failure 500 {object} map[string]string
// @Router /api/followers/can-comment [get]
func (h *FollowerHandler) CanComment(c *gin.Context) {
	followerId, err := strconv.ParseInt(c.Query("followerId"), 10, 64)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "invalid follower id"})
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
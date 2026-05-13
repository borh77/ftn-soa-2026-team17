package model

type FollowRequest struct {
	FollowerId int64 `json:"followerId"`
	FollowingId int64 `json:"followingId"`
}
package repository

import (
	"context"

	"github.com/neo4j/neo4j-go-driver/v5/neo4j"
)

type FollowerRepository struct {
	Driver neo4j.DriverWithContext
}

func NewFollowerRepository(driver neo4j.DriverWithContext) *FollowerRepository {
	return &FollowerRepository{
		Driver: driver,
	}
}

func (r *FollowerRepository) FollowUser(followerId int64, followingId int64) error {
	session := r.Driver.NewSession(context.Background(), neo4j.SessionConfig{})
	defer session.Close(context.Background())

	_, err := session.Run(
		context.Background(),
		`
		MERGE (follower:User {id: $followerId})
		MERGE (following:User {id: $followingId})
		MERGE (follower)-[:FOLLOWS]->(following)
		`,
		map[string]interface{}{
			"followerId": followerId,
			"followingId": followingId,
		},
	)

	return err
}

func (r *FollowerRepository) GetRecommendations(userId int64) ([]int64, error) {
	session := r.Driver.NewSession(context.Background(), neo4j.SessionConfig{})
	defer session.Close(context.Background())

	result, err := session.Run(
		context.Background(),
		`
		MATCH (u:User {id: $userId})-[:FOLLOWS]->(:User)-[:FOLLOWS]->(recommended:User)
		WHERE NOT (u)-[:FOLLOWS]->(recommended)
		AND u <> recommended
		RETURN DISTINCT recommended.id AS id
		`,
		map[string]interface{}{
			"userId": userId,
		},
	)

	if err != nil {
		return nil, err
	}

	var recommendations []int64

	for result.Next(context.Background()) {
		record := result.Record()

		id, _ := record.Get("id")

		recommendations = append(recommendations, id.(int64))
	}

	return recommendations, nil
}

func (r *FollowerRepository) UnfollowUser(followerId int64, followingId int64) error {
	session := r.Driver.NewSession(context.Background(), neo4j.SessionConfig{})
	defer session.Close(context.Background())

	_, err := session.Run(
		context.Background(),
		`
		MATCH (follower:User {id: $followerId})-[relationship:FOLLOWS]->(following:User {id: $followingId})
		DELETE relationship
		`,
		map[string]interface{}{
			"followerId":  followerId,
			"followingId": followingId,
		},
	)

	return err
}

func (r *FollowerRepository) IsFollowing(followerId int64, followingId int64) (bool, error) {
	session := r.Driver.NewSession(context.Background(), neo4j.SessionConfig{})
	defer session.Close(context.Background())

	result, err := session.Run(
		context.Background(),
		`
		MATCH (follower:User {id: $followerId})-[relationship:FOLLOWS]->(following:User {id: $followingId})
		RETURN COUNT(relationship) > 0 AS isFollowing
		`,
		map[string]interface{}{
			"followerId":  followerId,
			"followingId": followingId,
		},
	)

	if err != nil {
		return false, err
	}

	if result.Next(context.Background()) {
		value, _ := result.Record().Get("isFollowing")
		return value.(bool), nil
	}

	return false, nil
}


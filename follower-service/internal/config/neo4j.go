package config

import (
	"log"

	"github.com/neo4j/neo4j-go-driver/v5/neo4j"
)

func NewNeo4jDriver() neo4j.DriverWithContext {
	driver, err := neo4j.NewDriverWithContext(
		"neo4j://localhost:7687",
		neo4j.BasicAuth("neo4j", "password123", ""),
	)

	if err != nil {
		log.Fatal("Failed to create Neo4j driver:", err)
	}

	return driver
}
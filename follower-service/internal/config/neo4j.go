package config

import (
	"log"
	"os"

	"github.com/joho/godotenv"
	"github.com/neo4j/neo4j-go-driver/v5/neo4j"
)

func NewNeo4jDriver() neo4j.DriverWithContext {
	err := godotenv.Load()

	if err != nil {
		log.Println("No .env file found")
	}

	uri := os.Getenv("NEO4J_URI")
	username := os.Getenv("NEO4J_USERNAME")
	password := os.Getenv("NEO4J_PASSWORD")

	driver, err := neo4j.NewDriverWithContext(
		uri,
		neo4j.BasicAuth(username, password, ""),
	)

	if err != nil {
		log.Fatal("Failed to create Neo4j driver:", err)
	}

	return driver
}
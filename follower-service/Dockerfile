FROM golang:1.24-alpine AS build

WORKDIR /app

COPY go.mod go.sum ./
RUN go mod download

COPY . .

RUN go build -o follower-service ./cmd/server

FROM alpine:latest

WORKDIR /app

COPY --from=build /app/follower-service .

EXPOSE 8082

CMD ["./follower-service"]
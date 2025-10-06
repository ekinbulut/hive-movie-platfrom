# Hive Movie Platform - Podman Compose Management
# Colors for better UX
GREEN := \033[0;32m
YELLOW := \033[1;33m
RED := \033[0;31m
NC := \033[0m # No Color
BLUE := \033[0;34m

# Project configuration
PROJECT_NAME := hive-movie-platform
COMPOSE_FILE := docker-compose.yaml

.PHONY: help up down restart logs status clean clean-all build

# Default target - show help
help: ## Show this help message
	@echo "$(BLUE)Hive Movie Platform - Podman Compose Commands$(NC)"
	@echo "================================================="
	@awk 'BEGIN {FS = ":.*?## "} /^[a-zA-Z_-]+:.*?## / {printf "$(GREEN)%-15s$(NC) %s\n", $$1, $$2}' $(MAKEFILE_LIST)

# Start all services
up: ## Start all services using docker-compose
	@echo "$(YELLOW)Starting all services...$(NC)"
	@podman-compose -f $(COMPOSE_FILE) up -d
	@echo "$(GREEN)✓ All services started successfully!$(NC)"
	@echo "$(BLUE)Hive App: http://localhost:8080$(NC)"
	@echo "$(BLUE)Hive IDM: http://localhost:8082$(NC)"

# Stop all services
down: ## Stop all services using docker-compose
	@echo "$(YELLOW)Stopping all services...$(NC)"
	@podman-compose -f $(COMPOSE_FILE) down
	@echo "$(GREEN)✓ All services stopped$(NC)"

# Restart all services
restart: down up ## Restart all services

# Build all images
build: ## Build all images using docker-compose
	@echo "$(YELLOW)Building all images...$(NC)"
	@podman-compose -f $(COMPOSE_FILE) build
	@echo "$(GREEN)✓ All images built$(NC)"

# Show container logs
logs: ## Show logs for all containers
	@podman-compose -f $(COMPOSE_FILE) logs --tail=20

# Show logs for specific service
logs-app: ## Show Hive App logs
	@podman-compose -f $(COMPOSE_FILE) logs -f hive-app

logs-idm: ## Show Hive IDM logs
	@podman-compose -f $(COMPOSE_FILE) logs -f hive-idm

logs-watcher: ## Show Hive Watcher logs
	@podman-compose -f $(COMPOSE_FILE) logs -f hive-watcher

# Show status of all containers
status: ## Show status of all containers
	@echo "$(BLUE)Container Status:$(NC)"
	@echo "=================="
	@podman-compose -f $(COMPOSE_FILE) ps

# Clean up containers and volumes
clean: down ## Stop and remove containers, networks, and volumes
	@echo "$(YELLOW)Cleaning up containers and networks...$(NC)"
	@podman-compose -f $(COMPOSE_FILE) down --volumes --remove-orphans
	@echo "$(GREEN)✓ Cleanup completed$(NC)"

# Clean up everything including images
clean-all: clean ## Remove containers, networks, volumes, and images
	@echo "$(RED)⚠️  WARNING: This will remove all images and data!$(NC)"
	@read -p "Are you sure? (y/N): " confirm && [ "$$confirm" = "y" ] || exit 1
	@echo "$(YELLOW)Removing images...$(NC)"
	@podman rmi localhost/hive-app:latest localhost/hive-idm:latest localhost/hive-watcher:latest 2>/dev/null || true
	@podman system prune -a -f
	@echo "$(RED)✓ All images and data removed$(NC)"

# Enter container shell
shell-app: ## Enter Hive App container shell
	@podman-compose -f $(COMPOSE_FILE) exec hive-app /bin/bash

shell-idm: ## Enter Hive IDM container shell
	@podman-compose -f $(COMPOSE_FILE) exec hive-idm /bin/bash

shell-watcher: ## Enter Hive Watcher container shell
	@podman-compose -f $(COMPOSE_FILE) exec hive-watcher /bin/bash

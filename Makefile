.DEFAULT_GOAL := help

# General Variables
date=$(shell date +'%y.%m.%d.%H.%M')
project := Bumbershoot utils
container := src
docker-filecheck := /.dockerenv
docker-warning := ""
RED=\033[0;31m
GREEN=\033[0;32m
NC=\033[0m # No Color
version := 1.0.$(shell git rev-list HEAD --count)

dockerhub := rolfwessels/Bumbershoot

ifdef GITHUB_BASE_REF
	current-branch :=  $(patsubst refs/heads/%,%,${GITHUB_HEAD_REF})
else ifdef GITHUB_REF
	current-branch :=  $(patsubst refs/heads/%,%,${GITHUB_REF})
else 
	current-branch :=  $(shell git rev-parse --abbrev-ref HEAD)
endif

release := 'development'
ifeq ($(env), prod)
	release := 'production'
endif



ifeq ($(current-branch), master)
  version-tag :=  $(version)
else ifeq ($(current-branch), develop)
  version-tag := $(version)-beta
else
  version-tag := $(version)-alpha
endif

# Docker Warning
ifeq ("$(wildcard $(docker-filecheck))","")
	docker-warning = "⚠️  WARNING: Can't find /.dockerenv - it's strongly recommended that you run this from within the docker container."
endif

# Targets
help:
	@echo "The following commands can be used for building & running & deploying the the $(project) container"
	@echo "---------------------------------------------------------------------------------------------"
	@echo "Targets:"
	@echo "  Docker Targets (run from local machine)"
	@echo "   - up          : brings up the container & attach to the default container ($(container))"
	@echo "   - down        : stops the container"
	@echo "   - build       : (re)builds the container"
	@echo ""
	@echo "  Service Targets (should only be run inside the docker container)"
	@echo "   - publish      : Build the $(project) to build folder"
	@echo "   - version      : Set current version number $(project)"
	@echo "   - test         : Run the $(project) tests"
	@echo "   - deploy       : Deploy the $(project)"
	@echo ""
	@echo "Options:"
	@echo " - env    : sets the environment - supported environments are: dev | prod"
	@echo ""
	@echo "Examples:"
	@echo " - Start Docker Container              : make up"
	@echo " - Rebuild Docker Container            : make build"
	@echo " - Rebuild & Start Docker Container    : make build up"
	@echo " - Publish and deploy                  : make publish deploy env=dev"

up:
	@echo "Starting containers..."
	@docker-compose up -d
	@echo "Attachig shell..."
	@docker-compose exec $(container) bash

down:
	@echo "Stopping containers..."
	@docker-compose down

build: down
	@echo "Stopping containers..."
	@docker-compose down
	@echo "Building containers..."
	@docker-compose build

version:
	@echo "${GREEN}Setting version number $(version) ${NC}"
	@sed 's/Version>.*</Version>$(version-tag)</' src/Bumbershoot.Utilities/Bumbershoot.Utilities.csproj > src/Bumbershoot.Utilities/Bumbershoot.Utilities.csproj.ch  
	@mv  src/Bumbershoot.Utilities/Bumbershoot.Utilities.csproj.ch src/Bumbershoot.Utilities/Bumbershoot.Utilities.csproj

publish:  version
	@echo  "${GREEN}Publish branch $(current-branch) to $(version-tag)${NC}"
	dotnet build --configuration Release
	dotnet pack src/Bumbershoot.Utilities/Bumbershoot.Utilities.csproj
	dotnet nuget push src/Bumbershoot.Utilities/bin/Debug/Bumbershoot.Utilities.*.nupkg -k ${NUGET_KEY} -s https://api.nuget.org/v3/index.json


restore: 
	@echo -e "${GREEN}Restore $(project) nuget packages${NC}"
	dotnet restore

test: restore
	@echo -e "${GREEN}Testing the $(project)${NC}"
	export DOTNET_ENVIRONMENT "Development"
	dotnet test

deploy: docker-check env-check
	@echo -e "${GREEN}Deploying v${version} of $(release) release${NC}"

docker-check:
	$(call assert-file-exists,$(docker-filecheck), This step should only be run from Docker. Please run `make up` first.)

env-check:
	$(call check_defined, env, No environment set. Supported environments are: [ master | dev | prod ]. Please set the env variable. e.g. `make env=dev plan`)

# Check that given variables are set and all have non-empty values,
# die with an error otherwise.
#
# Params:
#   1. Variable name(s) to test.
#   2. (optional) Error message to print.
check_defined = \
    $(strip $(foreach 1,$1, \
    	$(call __check_defined,$1,$(strip $(value 2)))))
__check_defined = \
    $(if $(value $1),, \
    	$(error Undefined $1$(if $2, ($2))))

define assert
  $(if $1,,$(error Assertion failed: $2))
endef

define assert_warn
  $(if $1,,$(warn Assertion failed: $2))
endef

define assert-file-exists
  $(call assert,$(wildcard $1),$1 does not exist. $2)
endef

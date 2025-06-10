#!/bin/bash

CONTAINER_NAME=sqlserver2022

echo "Stopping and removing container..."
docker stop $CONTAINER_NAME
docker rm $CONTAINER_NAME

echo "Removing volume..."
docker volume rm sqlserver2022_data

echo "Cleanup complete."


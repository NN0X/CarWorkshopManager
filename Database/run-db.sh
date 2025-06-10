#!/bin/bash

IMAGE_NAME=custom-mssql
CONTAINER_NAME=sqlserver2022
SA_PASSWORD="<DB_PASSWORD_REDACTED>"
DB_NAME="WorkshopDb"

echo "🔧 Building Docker image..."
docker build -t $IMAGE_NAME .

echo "🚀 Starting SQL Server container..."
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=$SA_PASSWORD" \
  -p 1433:1433 --name $CONTAINER_NAME \
  -v sqlserver2022_data:/var/opt/mssql \
  -d $IMAGE_NAME

echo "⏳ Waiting for SQL Server to initialize..."
sleep 20

echo "🛠️ Creating database: $DB_NAME"
docker exec -i $CONTAINER_NAME sqlcmd \
  -S localhost -U sa -P "$SA_PASSWORD" \
  -Q "IF DB_ID('$DB_NAME') IS NULL CREATE DATABASE [$DB_NAME];"

echo "✅ SQL Server is running with '$DB_NAME' created."

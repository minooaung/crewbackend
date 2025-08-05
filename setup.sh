#!/bin/bash

set -e

echo "Deleting Migrations folder if exists..."
rm -rf Migrations || true

echo "Dropping existing database..."
dotnet ef database drop --force --no-build

echo "Adding initial migration..."
dotnet ef migrations add InitialCreate
#dotnet ef migrations add InitialCreate --no-build

echo "Updating database..."
dotnet ef database update

echo "Running the app..."
dotnet run

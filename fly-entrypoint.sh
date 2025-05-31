#!/bin/bash

# # Run migrations first
# echo "Running database migrations..."
# cd /app/migrator
# dotnet Dallal.DbMigrator.dll

# Start the web application
echo "Starting web application..."
cd /app/web
dotnet Dallal.Web.dll 
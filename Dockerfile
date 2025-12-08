# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore ExpenseTracker.csproj

# Copy everything else and build
COPY . ./
RUN dotnet publish ExpenseTracker.csproj -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the built application from the build stage
COPY --from=build /app/out .

# Expose port (Render will provide PORT env variable)
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "ExpenseTracker.dll"]

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

# The app listens on $PORT (see Program.cs). 8080 matches EXPOSE below, so a bare
# `docker run -p 8080:8080` works. Compose and Render override it with their own value.
ENV PORT=8080

EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "ExpenseTracker.dll"]

# Docker Setup - Expense Tracker Backend

How the backend is containerized and how to build/run it locally.

## Files

| File                    | Purpose                                                      |
| ----------------------- | ------------------------------------------------------------ |
| `Dockerfile`            | Multi-stage build (SDK build stage → ASP.NET runtime stage)   |
| `.dockerignore`         | Keeps `bin`, `obj`, `.env`, git and docs out of the build context |
| `render.yaml`           | Render Blueprint (optional deployment config)                |
| `../docker-compose.yml` | Runs the backend together with the frontend                  |

## How the image is built

1. **Build stage** (`mcr.microsoft.com/dotnet/sdk:8.0`)
    - Restores NuGet packages from `ExpenseTracker.csproj`
    - Publishes a Release build to `/app/out`

2. **Runtime stage** (`mcr.microsoft.com/dotnet/aspnet:8.0`)
    - Copies only the published output (no SDK in the final image)
    - Exposes port `8080`
    - `ENTRYPOINT ["dotnet", "ExpenseTracker.dll"]`

The app reads `PORT` from the environment (defaulting to `5000`) and calls
`ListenAnyIP`, so it works with Render's injected port.

**Benefits of the multi-stage build:** a much smaller final image (~100 MB vs
~500 MB), production-only dependencies and faster deployments.

## Build the image

```bash
cd expense-tracker-backend
docker build -t expense-tracker-backend .
```

## Run the container

```bash
docker run -p 8080:8080 \
  -e MONGODB_CONNECTION_STRING="mongodb+srv://..." \
  -e FRONTEND_URL="http://localhost:3000" \
  expense-tracker-backend
```

The API is available at http://localhost:8080

## Run with Docker Compose (recommended)

From the repository root, create a `.env` file from the example and start everything:

```bash
cp .env.example .env      # then fill in MONGODB_CONNECTION_STRING
docker compose up --build
```

Compose reads `.env` automatically. It starts the backend on http://localhost:8080
and the frontend on http://localhost:3000, and sets `FRONTEND_URL=http://localhost:3000`
so CORS works.

## Useful commands

```bash
docker compose logs -f backend    # follow backend logs
docker compose down               # stop and remove containers
docker compose build --no-cache   # rebuild from scratch
```

## Troubleshooting

- **"MongoDB connection string not configured"** - the `MONGODB_CONNECTION_STRING`
  environment variable is missing or empty.
- **Container exits immediately** - inspect it with `docker logs <container>`;
  usually a bad connection string or an unreachable Atlas cluster.
- **CORS blocked in the browser** - make sure `FRONTEND_URL` matches the browser
  origin exactly (scheme included, no trailing slash).

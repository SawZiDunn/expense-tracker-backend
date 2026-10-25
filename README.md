# Expense Tracker - Backend

RESTful API for the Expense Tracker app, built with ASP.NET Core 8 minimal APIs and
MongoDB.

The frontend lives at
[expense-tracker-frontend](https://github.com/SawZiDunn/expense-tracker-frontend).

## Features

- Full CRUD for expenses
- Filter expenses by date range
- Category totals, overall and per date range
- MongoDB storage - a local container or MongoDB Atlas
- Docker image for deployment

## Tech stack

- **Runtime** - .NET 8 / ASP.NET Core minimal APIs
- **Database** - MongoDB via `MongoDB.Driver` 3.5.2
- **Configuration** - `appsettings.json` plus environment variables, with DotNetEnv for a
  local `.env` file
- **Container** - multi-stage Dockerfile (SDK build image, ASP.NET runtime image)
- **API metadata** - OpenAPI via `WithOpenApi()`

## Run locally

Requires the .NET 8 SDK and a MongoDB instance - either Atlas or a local container.

```bash
git clone https://github.com/SawZiDunn/expense-tracker-backend
cd expense-tracker-backend

cat > .env <<'EOF'
MONGODB_CONNECTION_STRING=mongodb://localhost:27017
FRONTEND_URL=http://localhost:5173
EOF

dotnet restore
dotnet run
```

The API is available at **http://localhost:5000**.

`FRONTEND_URL` is the browser origin allowed by CORS - the Vite dev server by default. For
the database, put your Atlas URI in `MONGODB_CONNECTION_STRING`; nothing else changes.

## Configuration

| Variable                    | Required | Purpose                                                                          |
| --------------------------- | -------- | -------------------------------------------------------------------------------- |
| `MONGODB_CONNECTION_STRING` | yes      | MongoDB connection string. The app refuses to start without it.                   |
| `FRONTEND_URL`              | no       | Origin allowed by CORS. Comma-separated values allow several.                     |
| `CORS_ORIGINS`              | no       | Same as above, and takes precedence over `FRONTEND_URL`.                          |
| `PORT`                      | no       | Port Kestrel listens on. `5000` from source; the Docker image sets `8080`.        |
| `ASPNETCORE_ENVIRONMENT`    | no       | Defaults to `Production`. `Development` adds a `localhost:5173`/`5174` CORS fallback when no origins are set. |

The database and collection names are not part of the connection string - they come from
`appsettings.json`: database `ExpenseTrackerDB`, collection `expenses`.

For local runs these values can live in `.env`. In containers and on a hosting platform they
must be real environment variables; the `.env` file is not read when it isn't present.

## API

| Method | Endpoint                                           | Description                         |
| ------ | -------------------------------------------------- | ----------------------------------- |
| GET    | `/expenses`                                        | Get all expenses                    |
| GET    | `/expenses/{id}`                                   | Get expense by ID                   |
| POST   | `/expenses`                                        | Create new expense                  |
| PUT    | `/expenses/{id}`                                   | Update expense                      |
| DELETE | `/expenses/{id}`                                   | Delete expense                      |
| GET    | `/expenses/filter?start={date}&end={date}`         | Filter expenses by date range       |
| GET    | `/expenses/summary`                                | Category totals for all expenses    |
| GET    | `/expenses/summary/filter?start={date}&end={date}` | Category totals for a date range    |

### Examples

**Create an expense**

```http
POST /expenses
Content-Type: application/json

{
    "amount": 50.0,
    "category": "Food",
    "date": "2024-12-04T10:00:00Z",
    "note": "Lunch at restaurant"
}
```

```json
{
    "id": "507f1f77bcf86cd799439011",
    "amount": 50.0,
    "category": "Food",
    "date": "2024-12-04T10:00:00Z",
    "note": "Lunch at restaurant"
}
```

**Category totals**

```http
GET /expenses/summary
```

```json
{
    "Food": 250.5,
    "Transport": 120.0,
    "Entertainment": 80.0
}
```

A malformed id returns `400`, and an unknown id returns `404`.

## Run with Docker

```bash
docker build -t expense-tracker-backend .

docker run -p 8080:8080 \
  -e MONGODB_CONNECTION_STRING="mongodb+srv://user:pass@cluster.mongodb.net/" \
  -e FRONTEND_URL="http://localhost:3000" \
  expense-tracker-backend
```

The API is available at **http://localhost:8080**. The image sets `PORT=8080` to match its
`EXPOSE`, so the published port works as written; if you override `PORT`, publish the same
number.

The multi-stage build compiles with the .NET SDK image and runs the result on the smaller
ASP.NET runtime image, so the SDK never ships to production.

## Run the full stack

The frontend, this API and a MongoDB container are wired together in the deployment
repository: <https://github.com/SawZiDunn/expense-tracker-deployment>

```bash
git clone https://github.com/SawZiDunn/expense-tracker-deployment
cd expense-tracker-deployment
docker compose up -d
```

That starts the API on http://localhost:8080 and the frontend on http://localhost:3000,
with MongoDB data in a persistent volume. No `.env` and no Atlas account needed.

## Deploy

Render with MongoDB Atlas: see [DEPLOYMENT.md](DEPLOYMENT.md).

## License

MIT.

## Author

**Saw Zi Dunn** - [@SawZiDunn](https://github.com/SawZiDunn)

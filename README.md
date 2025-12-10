# Expense Tracker Backend API

A RESTful API built with ASP.NET Core 8.0 and MongoDB Atlas for managing personal expenses.
Although I've developed some backend services using FastAPI, Django, Express, etc, this is my first backend project using **C#**and **.NET**, featuring a modern minimal API approach with Docker containerization for seamless deployment.

Frontend for this project is available at: [Expense Tracker Frontend](https://github.com/SawZiDunn/expense-tracker-frontend)

## Features

-   Full CRUD operations for expense management
-   Filter expenses by date range
-   Category-based expense summaries with aggregation
-   MongoDB Atlas cloud database integration
-   Docker containerization for easy deployment
-   CORS configured for cross-origin requests

## Technologies

-   **Runtime**: .NET 8.0
-   **Framework**: ASP.NET Core Minimal API
-   **Database**: MongoDB Atlas (Cloud NoSQL)
-   **ODM**: MongoDB.Driver 3.5.2
-   **Configuration**: DotNetEnv for environment variables
-   **Containerization**: Docker (multi-stage build)
-   **Deployment**: Render (Platform-as-a-Service)

## Prerequisites

-   .NET 8.0 SDK
-   MongoDB Atlas account (free tier available)
-   Docker (optional, for containerized deployment)

## Quick Start

### Local Development

1. **Clone the repository**

    ```bash
    git clone https://github.com/SawZiDunn/expense-tracker-backend.git
    cd expense-tracker-backend
    ```

2. **Set up environment variables**

    Create a `.env` file in the project root:

    ```env
    MONGODB_CONNECTION_STRING=mongodb+srv://username:password@cluster.mongodb.net/
    FRONTEND_URL=http://localhost:5173
    ```

3. **Restore dependencies**

    ```bash
    dotnet restore
    ```

4. **Run the application**

    ```bash
    dotnet run
    ```

    The API will be available at `http://localhost:5000`

### Docker Deployment

1. **Build the Docker image**

    ```bash
    docker build -t expense-tracker-backend .
    ```

2. **Run the container**

    ```bash
    docker run -p 8080:8080 \
      -e MONGODB_CONNECTION_STRING="your-connection-string" \
      -e FRONTEND_URL="https://your-frontend.vercel.app" \
      expense-tracker-backend
    ```

3. **Access the API**

    API will be available at `http://localhost:8080`

## API Endpoints

### Expenses

| Method | Endpoint                                           | Description                         |
| ------ | -------------------------------------------------- | ----------------------------------- |
| GET    | `/expenses`                                        | Get all expenses                    |
| GET    | `/expenses/{id}`                                   | Get expense by ID                   |
| POST   | `/expenses`                                        | Create new expense                  |
| PUT    | `/expenses/{id}`                                   | Update expense                      |
| DELETE | `/expenses/{id}`                                   | Delete expense                      |
| GET    | `/expenses/filter?start={date}&end={date}`         | Filter expenses by date range       |
| GET    | `/expenses/summary`                                | Get category summary (all expenses) |
| GET    | `/expenses/summary/filter?start={date}&end={date}` | Get category summary by date range  |

### Request/Response Examples

**Create Expense (POST /expenses)**

```json
{
    "amount": 50.0,
    "category": "Food",
    "date": "2024-12-04T10:00:00Z",
    "note": "Lunch at restaurant"
}
```

**Response**

```json
{
    "id": "507f1f77bcf86cd799439011",
    "amount": 50.0,
    "category": "Food",
    "date": "2024-12-04T10:00:00Z",
    "note": "Lunch at restaurant"
}
```

**Filter by Date Range**

```
GET /expenses/filter?start=2024-01-01&end=2024-12-31
```

**Category Summary Response**

```json
{
    "Food": 250.5,
    "Transport": 120.0,
    "Entertainment": 80.0
}
```

## Configuration

### Database Configuration

The app uses MongoDB Atlas with the following settings:

-   **Database Name**: `ExpenseTrackerDB`
-   **Collection Name**: `expenses`

Configuration is loaded from environment variables with fallback to `appsettings.json`.

## Docker

This project uses a **multi-stage Dockerfile** for optimal image size and security:

1. **Build Stage**: Uses `mcr.microsoft.com/dotnet/sdk:8.0` to compile the application
2. **Runtime Stage**: Uses minimal `mcr.microsoft.com/dotnet/aspnet:8.0` to run the app

**Benefits:**

-   Smaller final image (~100MB vs ~500MB)
-   Production-only dependencies
-   Faster deployments
-   Enhanced security

## License

This project is open source and available under the MIT License.

## Author

**Saw Zi Dunn**

-   GitHub: [@SawZiDunn](https://github.com/SawZiDunn)

---

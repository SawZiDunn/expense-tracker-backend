# Expense Tracker Backend API

ASP.NET Core Minimal API with MongoDB Atlas for expense tracking.

## Features

-   ✅ Full CRUD operations for expenses
-   ✅ Filter expenses by date range
-   ✅ Category-based expense summaries
-   ✅ MongoDB Atlas integration
-   ✅ CORS enabled for React frontend
-   ✅ camelCase JSON serialization

## Prerequisites

-   .NET 8.0 SDK
-   MongoDB Atlas account

## Setup

1. **Update MongoDB Connection String**

    Edit `appsettings.json` and replace the connection string with your MongoDB Atlas credentials:

    ```json
    "ConnectionString": "mongodb+srv://<username>:<password>@<cluster>.mongodb.net/?retryWrites=true&w=majority"
    ```

2. **Restore Dependencies**

    ```bash
    dotnet restore
    ```

3. **Run the Application**

    ```bash
    dotnet run
    ```

    The API will be available at `http://localhost:5000` (or `https://localhost:5001`)

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

## Project Structure

```
backend/
├── Models/
│   └── Expense.cs           # Expense model
├── Services/
│   └── ExpenseService.cs    # MongoDB service layer
├── Settings/
│   └── MongoDbSettings.cs   # MongoDB configuration
├── Program.cs               # API endpoints & configuration
├── appsettings.json         # App configuration
└── ExpenseTracker.csproj    # Project file
```

## Technologies

-   ASP.NET Core 8.0 Minimal API
-   MongoDB.Driver 2.28.0
-   MongoDB Atlas (Database)

## CORS Configuration

Currently configured to allow requests from `http://localhost:5173` (React dev server). Update the CORS policy in `Program.cs` if needed.

# Deployment Guide - Expense Tracker Backend

Deploy the ASP.NET Core 8 + MongoDB backend to [Render](https://render.com) using Docker.

## Prerequisites

- MongoDB Atlas cluster
- Render account (free tier available)
- Code pushed to GitHub

## Step 1 - Prepare MongoDB Atlas

1. Sign in to [MongoDB Atlas](https://cloud.mongodb.com)
2. Create a database user (or reuse an existing one)
3. **Network Access** → **Add IP Address** → allow `0.0.0.0/0` (Render's IPs are dynamic)
4. **Database** → **Connect** → **Drivers** → copy the connection string, e.g.
   `mongodb+srv://username:password@cluster.mongodb.net/`

## Step 2 - Deploy to Render

### Option A - Render Dashboard (recommended)

1. **New +** → **Web Service**
2. Connect the `expense-tracker-backend` repository
3. Configure the service:

    - **Name**: `expense-tracker-backend`
    - **Region**: closest to you
    - **Branch**: `main`
    - **Runtime**: `Docker`
    - **Instance Type**: `Free`

4. **Advanced** → **Add Environment Variable**:

    ```
    MONGODB_CONNECTION_STRING=mongodb+srv://username:password@cluster.mongodb.net/
    FRONTEND_URL=https://your-frontend-url.vercel.app
    ASPNETCORE_ENVIRONMENT=Production
    ```

5. **Create Web Service**

### Option B - Blueprint (`render.yaml`)

The repository ships a `render.yaml`:

1. Update the `FRONTEND_URL` value in `render.yaml`
2. Push to GitHub
3. In Render: **New +** → **Blueprint** → connect the repository
4. Render reads `render.yaml` and creates the service automatically

## Step 3 - Verify the deployment

Render gives you a URL such as `https://expense-tracker-backend.onrender.com`.

```bash
# Get all expenses
curl https://your-backend-url.onrender.com/expenses

# Create an expense
curl -X POST https://your-backend-url.onrender.com/expenses \
  -H "Content-Type: application/json" \
  -d '{"amount": 100, "category": "food", "note": "test"}'
```

## Step 4 - Connect the frontend

1. Set the frontend's `VITE_API_URL` to this Render URL and redeploy the frontend
2. Set `FRONTEND_URL` **here** to the deployed frontend URL, then redeploy — this
   is what allows the browser to call the API (CORS)

## Environment Variables

| Variable                    | Description                              | Example                                        |
| --------------------------- | ---------------------------------------- | ---------------------------------------------- |
| `MONGODB_CONNECTION_STRING` | MongoDB Atlas connection string          | `mongodb+srv://user:pass@cluster.mongodb.net/` |
| `FRONTEND_URL`              | Frontend origin allowed by CORS          | `https://myapp.vercel.app`                     |
| `ASPNETCORE_ENVIRONMENT`    | ASP.NET Core environment                 | `Production`                                   |
| `PORT`                      | Port Kestrel listens on (Render sets it) | `8080`                                         |

Locally, the values can live in a `.env` file (see `.env.example`). In production
they must be set as real environment variables — never committed.

## Local Docker check

Test the production image before deploying. See `DOCKER_SETUP.md` for details.

```bash
docker build -t expense-tracker-backend .

docker run -p 8080:8080 \
  -e MONGODB_CONNECTION_STRING="your-connection-string" \
  -e FRONTEND_URL="http://localhost:5173" \
  expense-tracker-backend

curl http://localhost:8080/expenses
```

## Troubleshooting

### Container fails to start

- Check the Render logs for the specific error message
- Verify the MongoDB connection string is correct
- Confirm Atlas network access allows connections from `0.0.0.0/0`

### CORS errors

- Add the exact frontend origin to `FRONTEND_URL`
- The origin includes the scheme and must not have a trailing slash
- Redeploy the backend after changing environment variables

### MongoDB authentication errors

- Verify the username and password
- URL-encode special characters in the password
- Confirm the database user has read/write permissions

## Important Notes

- **Free tier limitations**: Render's free instance spins down after ~15 minutes of inactivity
- **Cold starts**: the first request after spin-down can take 30-60 seconds
- **MongoDB**: keep the connection string secure, never commit it to GitHub
- **HTTPS**: Render provides TLS for your service automatically

## Monitoring

Open the service in the Render Dashboard and use the **Logs** tab to watch
real-time application output.

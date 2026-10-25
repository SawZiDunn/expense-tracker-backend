# Deployment - Expense Tracker Backend

Deploy the API to [Render](https://render.com) as a Docker service, with
[MongoDB Atlas](https://www.mongodb.com/atlas) as the database.

The repository already contains what Render needs: a `Dockerfile` and a `render.yaml`
blueprint.

## 1. Prepare MongoDB Atlas

1. Sign in to [MongoDB Atlas](https://cloud.mongodb.com)
2. Create a database user (or reuse one)
3. **Network Access** → **Add IP Address** → allow `0.0.0.0/0`
4. **Database** → **Connect** → **Drivers** → copy the connection string

Render's outbound IP addresses are dynamic, so the cluster has to accept connections from
anywhere unless you pay for a static egress IP.

## 2. Deploy to Render

### Option A - Dashboard

1. **New +** → **Web Service**
2. Connect the `expense-tracker-backend` repository
3. Configure it:

    - **Name**: `expense-tracker-backend`
    - **Region**: closest to you
    - **Branch**: `main`
    - **Runtime**: `Docker`
    - **Instance Type**: `Free`

4. Under **Environment**, add:

    ```
    MONGODB_CONNECTION_STRING=mongodb+srv://user:password@cluster.mongodb.net/
    FRONTEND_URL=https://your-frontend-url.vercel.app
    ```

5. **Create Web Service**

Do not set `PORT` - Render injects it and the container listens on it.

### Option B - Blueprint

The repository ships `render.yaml`:

1. Change the `FRONTEND_URL` value in `render.yaml`
2. Push to GitHub
3. Render: **New +** → **Blueprint** → connect the repository

Render reads the blueprint and creates the service.

## 3. Verify

Render gives you a URL such as `https://expense-tracker-backend.onrender.com`.

```bash
curl https://your-backend-url.onrender.com/expenses

curl -X POST https://your-backend-url.onrender.com/expenses \
  -H "Content-Type: application/json" \
  -d '{"amount": 100, "category": "food", "note": "test"}'
```

## 4. Connect the frontend

1. Set the frontend's `VITE_API_URL` to the Render URL and redeploy the frontend
2. Set `FRONTEND_URL` here to the frontend origin and redeploy the API

Step 2 is what allows the browser to call the API. Without it the request fails CORS, and
`FRONTEND_URL` must match the origin exactly - scheme included, no trailing slash.

## Environment variables

| Variable                    | Description                                        |
| --------------------------- | -------------------------------------------------- |
| `MONGODB_CONNECTION_STRING` | MongoDB Atlas connection string (required)         |
| `FRONTEND_URL`              | Frontend origin allowed by CORS                    |
| `CORS_ORIGINS`              | Comma-separated origins; overrides `FRONTEND_URL`  |
| `ASPNETCORE_ENVIRONMENT`    | Optional. The container defaults to `Production`.  |
| `PORT`                      | Set by Render. Do not define it yourself.          |

Never commit a real connection string. Locally these values can live in `.env`; on Render
they must be set as environment variables in the dashboard or in `render.yaml`.

## Troubleshooting

| Symptom                                  | Cause                                                        |
| ---------------------------------------- | ------------------------------------------------------------ |
| Deploy fails, logs show a connection error | `MONGODB_CONNECTION_STRING` is missing, wrong, or Atlas is blocking Render's IPs |
| Browser reports a CORS error             | `FRONTEND_URL` doesn't match the frontend origin exactly      |
| First request after a while is slow      | Free instances sleep when idle; the first request wakes them  |
| `502` immediately after deploy           | The container crashed at startup - check the logs for the MongoDB error |

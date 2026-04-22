# AirAware

AirAware is a small distributed aviation dashboard built around a .NET backend and a Next.js frontend. The repo ingests live flight data from AviationStack, enriches each flight with weather-based stress analysis from a separate gRPC service, stores the results in PostgreSQL, and renders them in a fleet dashboard.

## Run It

From the repo root:

```bash
docker compose up --build
```

That is the normal local startup path. The API now applies pending EF Core migrations automatically when it boots, so no extra migration command is needed for routine local runs.

## What Is In This Repo

- `src/AirAware.API`
  Main ASP.NET Core Web API. It fetches flights from AviationStack, stores flights and stress reports in PostgreSQL, and calls the weather service over gRPC.
- `src/AirAware.Weather`
  ASP.NET Core weather-analysis service. It calls OpenWeather, calculates a stress score, and returns that result to the API over gRPC.
- `src/AirAware.Shared`
  Shared models and the protobuf contract used by both backend services.
- `src/airaware.web`
  Next.js dashboard that lists flights and their latest stress analysis.
- `.github/workflows`
  CI that builds the .NET solution and verifies the Docker images build.

## Current Tech Stack

- Backend: C#, .NET 8, ASP.NET Core Web API
- ORM / data access: Entity Framework Core 8, Npgsql
- Inter-service communication: gRPC with Protocol Buffers
- Database: PostgreSQL 15
- Frontend: Next.js 16.1.6, React 19.2.3, TypeScript, Tailwind CSS 4, Lucide React
- Local orchestration: Docker Compose
- CI: GitHub Actions

## Current Constraints

- The frontend calls the API at `http://localhost:5077`.
- The API gRPC client is hardcoded to `http://weather-service:5222`, which makes Docker the primary supported run path for the full stack.

## How The System Works

1. The dashboard in `src/airaware.web` loads flights from `GET /api/Flights`.
2. When you trigger a sync, the frontend calls `POST /api/Flights/sync`.
3. `AirAware.API` fetches up to 10 active flights from AviationStack.
4. For each new flight, the API calls the weather service through gRPC.
5. `AirAware.Weather` looks up current weather from OpenWeather and computes a stress score based on temperature, wind, flight duration, and a small hash-based variance rule.
6. The API stores the flight plus its stress report in PostgreSQL.
7. The frontend renders the stored results from the API.

## Runtime Ports

- Frontend: `http://localhost:3000`
- API: `http://localhost:5077`
- API Swagger: `http://localhost:5077/swagger`
- Weather service HTTP / Swagger: `http://localhost:5119`
- Weather service gRPC: `localhost:5222`
- PostgreSQL: `localhost:5433`

## Prerequisites

For the main Docker-first workflow:

- Docker Desktop
- Docker Compose

Optional, only if you want to develop outside Docker or create new EF migrations:

- .NET 8 SDK
- Node.js 20+

## Environment Setup

Create a root `.env` file using `.env.example` as the template, then set:

```text
AVIATION_STACK_KEY=your_aviationstack_key
OPEN_WEATHER_KEY=your_openweather_key
```

`docker compose` reads the root `.env` automatically.

## Recommended Way To Run AirAware

### 1. Start the stack

```bash
docker compose up --build
```

### 2. Open the app

- Dashboard: `http://localhost:3000`
- API Swagger: `http://localhost:5077/swagger`
- Weather Swagger: `http://localhost:5119/swagger`

### 3. Load data

Use the `Sync Fleet` button in the dashboard, or call the API directly:

```bash
curl -X POST http://localhost:5077/api/Flights/sync
```

Then inspect the stored flights:

```bash
curl http://localhost:5077/api/Flights
```

## Useful Commands

Start:

```bash
docker compose up --build
```

Start in detached mode:

```bash
docker compose up --build -d
```

Stop:

```bash
docker compose down
```

Stop and delete the Postgres volume too:

```bash
docker compose down -v
```

View logs:

```bash
docker compose logs -f
```

## Optional Host-Side Development Notes

The frontend can be run locally from `src/airaware.web` with:

```bash
npm install
npm run dev
```

The backend services are not fully host-first right now. The API's gRPC client is hardcoded to `weather-service:5222`, so the simplest supported full-stack workflow remains Docker Compose.

## API Surface

### API service

- `GET /api/Flights`
  Returns stored flights and their attached stress reports.
- `POST /api/Flights/sync`
  Pulls active flights from AviationStack, enriches them, and stores new rows.

### Weather service

- `GET /api/WeatherTest/analyze`
  Simple HTTP test endpoint for the weather scoring logic.
- gRPC `WeatherProcessor.GetStressScore`
  Internal endpoint used by the API.

## Project Structure

```text
AirAware/
|-- .github/
|   `-- workflows/
|-- src/
|   |-- AirAware.API/
|   |-- AirAware.Shared/
|   |-- AirAware.Weather/
|   `-- airaware.web/
|-- .env.example
|-- docker-compose.yml
`-- README.md
```

## Verification Notes

These checks pass from the current repo state:

- `dotnet build src/AirAware.sln -m:1`
- `npm run build` in `src/airaware.web`
- `docker compose config`

## Known Gaps

- `src/AirAware.API/AirAware.API.http` and `src/AirAware.Weather/AirAware.Weather.http` should be used for the real endpoints in this repo, not the deleted `weatherforecast` sample endpoint.
- There are no automated tests in the repo yet. CI currently verifies builds, not behavior.

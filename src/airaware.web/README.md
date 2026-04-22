# AirAware Web

This is the frontend dashboard for AirAware. It is built with Next.js 16, React 19, TypeScript, and Tailwind CSS 4.

## What It Does

- Calls `GET http://localhost:5077/api/Flights` to render the fleet list
- Calls `POST http://localhost:5077/api/Flights/sync` when you press `Sync Fleet`
- Displays flight status, origin and destination, and the most recent stress analysis

## Fastest Way To Start The App

From the repo root:

```bash
docker compose up --build
```

## Scripts

From this directory:

```bash
npm install
npm run dev
npm run build
npm run start
```

## Recommended Workflow

The primary supported way to run the whole project is from the repo root:

```bash
docker compose up --build
```

That path keeps the frontend aligned with the API and weather service without any extra local configuration.

## Important Runtime Assumption

The frontend currently expects the API at:

```text
http://localhost:5077
```

If that API URL changes, update `app/page.tsx` or introduce environment-based frontend configuration.

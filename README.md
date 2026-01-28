# AirAware: Distributed Aviation Logistics & Fleet Manager

AirAware is a microservices-based predictive maintenance platform designed to manage the structural health of a global aircraft fleet in real-time. Moving away from traditional fixed-schedule maintenance, AirAware utilizes high-frequency data ingestion and environmental enrichment to calculate a "Stress Score" for every landing. It demonstrates high-performance service-to-service communication and enterprise-grade cloud-native architecture.

## Architecture

The system follows a distributed microservices architecture designed for low-latency data processing and high reliability:

1.  **Main API (.NET 8):** The central orchestrator responsible for ingesting live flight data from the AviationStack "Firehose." It manages the PostgreSQL state store and coordinates analysis requests.
2.  **Weather Analysis Service (.NET 8):** A specialized logic engine that calculates environmental stress. It performs "Smart Search" city-cleaning logic to fetch contextual weather data (Temperature, Wind Speed, Visibility) from OpenWeatherMap.
3.  **gRPC Bridge (Protobuf):** A high-performance, binary communication layer. The Main API talks to the Weather Service via Google Remote Procedure Calls (gRPC) rather than standard HTTP to ensure the lowest possible latency for internal requests.
4.  **State Store (PostgreSQL):** A persistent relational database that stores historical flight logs, aircraft status, and accumulated stress reports.
5.  **API Shield (Redis):** An in-memory cache used to shield external APIs from redundant calls and manage system state.
6.  **Fleet Dashboard (Next.js 15 + Tailwind):** A professional, dark-themed command center that visualizes fleet health, real-time locations, and maintenance priorities.

## Tech Stack

*   **Languages:** C# (.NET 8), TypeScript (Frontend), Protocol Buffers (gRPC)
*   **Backend Framework:** ASP.NET Core Web API, Entity Framework Core (ORM)
*   **Communication:** gRPC (High-speed Service-to-Service)
*   **Database:** PostgreSQL 15, Redis
*   **Frontend:** Next.js 15, Tailwind CSS, Lucide Icons
*   **Orchestration:** Docker, Docker Compose
*   **Infrastructure:** Terraform, AWS (EKS, RDS, S3)
*   **CI/CD:** GitHub Actions

## Project Structure

```text
AirAware/
├── src/
│   ├── AirAware.API/       # Main Orchestration Service
│   ├── AirAware.Weather/   # gRPC Weather Logic Service
│   ├── AirAware.Shared/    # Proto Contracts & Common Models
│   └── airaware.web/       # Next.js Dashboard
├── .github/                # CI/CD Workflows (Docker Verification)
├── infra/                  # Terraform Infrastructure as Code
├── deploy/                 # Kubernetes Manifests
├── .env                    # Local Secrets (API Keys)
└── docker-compose.yml      # Multi-container Orchestration
```

## Getting Started

### 1. Environment Configuration
Create a `.env` file in the root directory to store your secure API keys:
```text
AVIATION_STACK_KEY=your_key_here
OPEN_WEATHER_KEY=your_key_here
```

### 2. Infrastructure Launch
Spin up the entire distributed stack (Database, Cache, Microservices, and Frontend) with a single command:
```bash
docker compose up --build -d
```

### 3. Database Initialization
Push the relational schema into the Dockerized PostgreSQL instance:
```bash
dotnet ef database update --project src/AirAware.API/ --startup-project src/AirAware.API/
```

## System Features

### High-Speed gRPC Orchestration
AirAware utilizes gRPC for internal microservice communication. By using binary serialization and HTTP/2, the system minimizes the overhead of cross-service data enrichment, allowing the Fleet Manager to see "Landed" status and "Stress Analysis" almost simultaneously.

### Predictive Stress Engine
The system applies a complex logic engine to every flight. It calculates "Hull Stress" by combining real-world environmental factors (Extreme Heat, High Winds, Flight Duration) with a "Hardware Aging" algorithm. This allows the dashboard to categorize aircraft into three maintenance tiers: **Healthy**, **Routine Check Required**, or **Immediate Critical Inspection**.

## CI/CD
This project utilizes an enterprise GitHub Actions pipeline to ensure system integrity. Every push to the main branch triggers a two-stage verification:
1.  **Compilation Check:** Validates the .NET solution and TypeScript frontend.
2.  **Docker Registry Verification:** Verifies that all Dockerfiles build correctly, ensuring the system is always "Cloud Ready" for deployment to AWS EKS.
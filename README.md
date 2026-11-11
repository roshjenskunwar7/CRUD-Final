@'
# CRUD-Final — Student Management System

**Author:** Roshjens Kunwar  
**Course:** IT Security Management and DevOps

## 1. Project Description

A Student Management CRUD REST API developed using ASP.NET Core and C#. The application uses MariaDB for persistent data storage and is containerized with Docker and deployed to Kubernetes using Minikube.

## 2. Technologies

- C# and ASP.NET Core (.NET 10)
- MariaDB 11.4
- Docker and Docker Compose
- Kubernetes and Minikube
- k6 load testing
- PowerShell

## 3. Application Features

- Create a student record
- Retrieve all student records
- Retrieve a student by ID
- Update a student record
- Delete a student record
- Health endpoint at `/health`
- StudentHub web interface

### API Endpoints

| Method | Endpoint | Purpose |
|---|---|---|
| POST | `/api/students` | Create a student |
| GET | `/api/students` | Retrieve all students |
| GET | `/api/students/{id}` | Retrieve one student |
| PUT | `/api/students/{id}` | Update a student |
| DELETE | `/api/students/{id}` | Delete a student |
| GET | `/health` | Application health check |

## 4. Docker Setup

The application uses a multi-stage Dockerfile:

- .NET SDK image for building and publishing.
- ASP.NET runtime image for running the application.
- Application listens on port 8080.
- The final container runs as a non-root user.

Docker Compose starts:
- The CRUD application.
- MariaDB database.
- A named Docker volume for database persistence.
- A dedicated Docker network.
- An application health check.

Start the services:

```powershell
docker compose up -d --build

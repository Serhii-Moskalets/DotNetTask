# DotNetTask API - Backend

This is the server-side component of the DotNetTask ecosystem, built with **ASP.NET Core 10**. It provides a robust RESTful API for task management, security, and real-time data processing.

## Tech Stack
* **Framework:** .NET 10 (Web API)
* **Database:** PostgreSQL
* **ORM:** Entity Framework Core
* **Security:** Custom JWT Implementation & BCrypt.Net
* **Email Service:** FluentEmail / SMTP Integration
* **API Documentation:** Swagger / OpenAPI

## Prerequisites
Before running the application, ensure you have:
* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) installed.
* Docker Desktop (for running PostgreSQL via container).
* A tool for testing API endpoints (Postman, Insomnia, or the built-in Swagger UI).

## Configuration
The API requires specific environment variables or `appsettings.json` configurations to function correctly:

> [!WARNING]
> All keys and passwords in this documentation are **dummy data** for demonstration purposes only. 
> Please replace them with your own credentials using **User Secrets** to keep your development environment secure.

**1. Database Connection**
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=DotNetTaskDb;Username=postgres;Password=your_password"
}
```
**2. Security (JWT)**
```json
"JwtSettings": {
  "Secret": "your_very_long_and_secure_secret_key",
  "Issuer": "DotNetTaskAPI",
  "Audience": "DotNetTaskClient"
}
```
**3. Email Settings**
<br>Required for account verification and password resets.
```json
"EmailSettings": {
  "Host": "smtp.your-provider.com",
  "Port": 587,
  "UserName": "your-email@example.com",
  "Password": "your-app-password"
}
```

## Getting Started

**1. Start the Database:**
```bash
docker-compose up -d
```
**2. Apply Migrations:**
```bash
cd src/TodoListApp.Api
dotnet ef database update
```
**3. Run the Api:**
```bash
dotnet run
```
>The API will be available at: https://localhost:7000 or http://localhost:5105

## Project Architecture

**The backend follows a specialized architecture to ensure maintainability:**
* **Controllers:** *Handle HTTP requests and routing.*
* **Middleware:** *Includes `GlobalExceptionHandler` for centralized error handling and `UserSecurityMiddleware` for identity validation.*
* **Services:** *Contain business logic for tasks, tags, and comments.*
* **Data Access:** *EF Core repositories and PostgreSQL integration.*

##
***Developed by Serhii Moskalets. Version 1.0.0***
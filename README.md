# DotNetTask API

**DotNetTask API** is a professional full-stack task management ecosystem built with **.NET 10** and **React**. This project demonstrates a modern Web API implementation focused on clean architecture, robust security, and seamless frontend integration.

[![.NET Core CI](https://github.com/Serhii-Moskalets/DotNetTask/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/Serhii-Moskalets/DotNetTask/actions)

## Overview

This is more than just a to-do list; it is a scalable productivity engine. The system supports complex entity relations, custom identity management, and external service integrations (SMTP/Email).

**Key Terminology & Synonyms:**
* **Tasks** - *assignments, duties, objectives, backlog items.*
* **Security** - *protection, authentication, authorization, access control.*
* **Workflow** - *pipeline, development cycle, operations.*

## Solution Architecture

The project is organized as a monorepo, maintaining a clear separation between the server-side logic and the client-side interface:

| Component | Tech Stack | Description |
| :--- | :--- | :--- |
| **[Backend](./Backend/)** | .NET 10, EF Core, PostgreSQL | Core API, Business Logic, JWT Security |
| **[Frontend](./Frontend/todo-frontend/)** | React, Vite, SCSS | User Interface, Responsive Design, State Management |

## Key Technical Features

* **Custom JWT Identity System:** A bespoke security implementation using JSON Web Tokens and BCrypt for password hashing (independent of standard ASP.NET Core Identity).
* **Advanced Task Engine:** Comprehensive management of tasks including tags, hierarchical comments, priorities, and statuses.
* **Granular Access Control:** A specialized "User Task Access" mechanism for collaborative task management.
* **Automated Notification System:** Integrated SMTP engine for account verification, security alerts, and password recovery.
* **Persistent Storage:** High-performance PostgreSQL integration with automated schema migrations via Entity Framework Core.
* **CI/CD Pipeline:** Automated Build & Test workflows powered by GitHub Actions.

## Quick Start

1. Clone the repository:
   ```bash
   git clone https://github.com/Serhii-Moskalets/DotNetTask.git
   cd DotNetTask
   ```
2. Spin up the Infrastructure:
   <br>Use Docker to launch the PostgreSQL database instantly:
   ```bash
   cd Backend
   docker-compose up -d
   ```
3. Detailed Setup Guides:
   For specific configuration and local deployment, refer to:
   * **[Backend API Documentation](./Backend/README.md/)** - *DB migrations & .NET setup.*
   * **[Frontend Client Documentation](./Frontend/todo-frontend/README.md/)** - *Vite & React deployment.*

## Development Workflow
 **The project follows a strict branching model to ensure code quality:**
 * **main** - *Stable production-ready code.*
 * **dev** - *Integration branch for testing.*

##
***Developed and maintained by Serhii Moskalets.***

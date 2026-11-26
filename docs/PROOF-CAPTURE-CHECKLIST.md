# Portfolio Evidence and Screenshot Index

This document is the evidence register for the IT Service Desk Management System. The original images are kept in `screenshots/`; the sections below embed the most relevant evidence so reviewers can inspect it without hunting through the repository.

## Quick evidence summary

| Area | Evidence files |
|---|---|
| User interface | Login, User dashboard, raise request, Admin dashboard, audit history, request details |
| API documentation | Development Swagger screenshot |
| Azure infrastructure | Static Web Apps, App Service, Azure SQL resource overviews |
| Backend CI/CD | Successful API workflow screenshot and linked successful run |
| Frontend CI/CD | Successful Static Web Apps workflow screenshot and linked successful run |
| Live application | Registration, User dashboard, Admin dashboard |
| Database | Azure SQL schema/tables and demonstration records |

## 1. Application UI evidence

### Login

![Application login page](01-login.png)

### User dashboard

![User dashboard](02-user-dashboard.png)

### Raise service request

![Raise service request form](03-raise-request.png)

### Admin dashboard

![Admin dashboard](04-admin-dashboard.png)

### Audit history

![Audit history screen](05-audit-history.png)

### Request details

![Service request details](06-request-details.png)

### Swagger / OpenAPI documentation (Development)

![Swagger API documentation](07-swagger-api.png)

## 2. Azure infrastructure proof

### Azure Static Web Apps

![Azure Static Web Apps resource](08-azure-static-web-app-overview.png)

### Azure App Service

![Azure App Service resource](09-azure-app-service-overview.png)

### Azure SQL Database

![Azure SQL Database resource](10-azure-sql-database-overview.png)

## 3. CI/CD deployment proof

### Backend workflow — successful deployment

![Backend GitHub Actions run](11-github-api-actions-success.png)

[Open the verified successful backend workflow run #3](https://github.com/Neeraj-pp/IT-Service-Desk-Management-System/actions/runs/45583275795). The run completed both **Build, Test and Publish API** and **Deploy API to Azure App Service** successfully. Its job steps include automated tests, Release publish, artifact upload, Azure OIDC login, and App Service deployment.

### Frontend workflow — successful deployment

![Frontend GitHub Actions run](12-github-frontend-actions-success.png)

[Open the verified successful frontend workflow run #10](https://github.com/Neeraj-pp/IT-Service-Desk-Management-System/actions/runs/45761022603). The **Build and Deploy Job** completed successfully.

**Current-status clarification:** A later frontend run #11 ([view run](https://github.com/Neeraj-pp/IT-Service-Desk-Management-System/actions/runs/45761027414)) completed the React/Oryx build and uploaded the artifact, but Azure returned **Deployment Canceled** during deployment polling. Run #10 is the successful frontend deployment proof; run #11 is not a successful deployment. Always inspect the live Actions page for the latest status before claiming the newest pipeline is green.

## 4. Live application verification

### Successful live user registration

![Live user registration](13-live-user-registration.png)

### Live User dashboard

![Live User dashboard](14-live-user-dashboard.png)

### Live Admin dashboard

![Live Admin dashboard](15-live-admin-dashboard.png)

## 5. Azure SQL database proof

### Database schema and tables

![Azure SQL schema and tables](16-azure-sql-schema-tables.png)

### Persisted demonstration records

![Azure SQL demo records](14-azure-sql-demo-records.png)

## Complete screenshot inventory

| File | What it shows |
|---|---|
| `01-login.png` | Original login interface |
| `02-user-dashboard.png` | Original User dashboard |
| `03-raise-request.png` | Service request creation form |
| `04-admin-dashboard.png` | Original Admin dashboard |
| `05-audit-history.png` | Audit history interface |
| `06-request-details.png` | Request details view |
| `07-swagger-api.png` | Development Swagger/OpenAPI UI |
| `08-azure-static-web-app-overview.png` | Azure Static Web Apps resource overview |
| `09-azure-app-service-overview.png` | Azure App Service resource overview |
| `10-azure-sql-database-overview.png` | Azure SQL Database resource overview |
| `11-github-api-actions-success.png` | Backend GitHub Actions success screenshot |
| `12-github-frontend-actions-success.png` | Frontend GitHub Actions success screenshot |
| `13-live-user-registration.png` | Live user registration |
| `14-azure-sql-demo-records.png` | SQL query results for demo records |
| `14-live-user-dashboard.png` | Live User dashboard |
| `15-live-admin-dashboard.png` | Live Admin dashboard |
| `16-azure-sql-schema-tables.png` | Azure SQL schema/table view |

**Filename note:** Two files use the numeric prefix `14`; their full paths are different and both are referenced explicitly. No files were renamed.

## Evidence safety checklist

Before adding or replacing screenshots, confirm they do not expose:
- passwords, password hashes, JWTs, cookies, access tokens, or session values;
- SQL connection strings, SQL passwords, or JWT signing keys;
- GitHub Actions secret values or Azure deployment tokens;
- unnecessary subscription/tenant identifiers, personal email addresses, phone numbers, or billing information.

Use synthetic users and request content for public demonstrations. A screenshot of an Azure resource or a historical successful workflow is point-in-time evidence, not a promise of current uptime.

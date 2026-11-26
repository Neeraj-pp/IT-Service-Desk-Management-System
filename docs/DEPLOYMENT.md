# Azure Deployment Guide

This guide documents the current hosting layout and the GitHub Actions deployment process for the IT Service Desk Management System.

## Live endpoints

| Component | Current deployment |
|---|---|
| React frontend | https://proud-stone-0019ac600.4.azurestaticapps.net |
| API base URL | https://servicedesk-api-neeraj-2026-fkgseadcemc2gpf0.centralindia-01.azurewebsites.net/api |
| API route prefix | /api/ITSRPAPI |
| Azure resource group | rg-servicedesk-portfolio |
| App Service | servicedesk-api-neeraj-2026 |
| Static Web App | proud-stone-0019ac600 |
| SQL logical server | sql-servicedesk-neeraj-2026 |
| SQL database | sqldb-servicedesk |

Do not treat these URLs as a permanent uptime guarantee. The resources are on learning/free tiers and can be paused or disabled by quota, subscription, or platform conditions.

## Architecture

~~~mermaid
flowchart TD
    Browser[User / Admin Browser]
    Frontend[Azure Static Web Apps<br/>React + Vite]
    Backend[Azure App Service<br/>Linux, ASP.NET Core .NET 10]
    Auth[JWT validation<br/>Role and ownership checks]
    Data[Repository + Entity Framework Core]
    Database[(Azure SQL Database)]

    Browser -->|HTTPS| Frontend
    Frontend -->|Axios + HTTPS + JWT| Backend
    Backend --> Auth
    Auth --> Data
    Data --> Database
~~~

The frontend and API are separate deployments. The API connects to Azure SQL with a dedicated SQL application login rather than the SQL Server administrator login.

## Azure App Service configuration

The API App Service uses the Linux .NET 10 runtime and currently runs on the F1 Free App Service plan.

Application settings are configured in **App Service → Settings → Environment variables**. The important keys are:

| App setting | Purpose |
|---|---|
| ASPNETCORE_ENVIRONMENT | Set to Production |
| Database__Provider | Set to SqlServer |
| ConnectionStrings__DefaultConnection | Azure SQL connection string; keep its value secret |
| Jwt__Issuer | JWT issuer |
| Jwt__Audience | JWT audience |
| Jwt__Key | Production JWT signing key (secret, at least 32 bytes) |
| Jwt__AccessTokenMinutes | Token lifetime; application validates 5–120 minutes |
| Frontend__AllowedOrigin | Exact Static Web Apps origin, without a trailing slash |

If the database has not yet been seeded with an Admin account, also set **AdminSeed__UserName** and **AdminSeed__Password** securely for first-time provisioning. Do not leave initial credentials in source control or public documentation. The seeder checks for an existing Admin before requiring these values on later starts.

### CORS

The production **Frontend__AllowedOrigin** is configured to match the Static Web Apps origin:

~~~text
https://proud-stone-0019ac600.4.azurestaticapps.net
~~~

If the Static Web Apps hostname changes, update the API's **Frontend__AllowedOrigin** setting and save/apply the App Service configuration.

## Azure SQL configuration

Production database configuration uses:

- Provider: SQL Server
- Database: sqldb-servicedesk
- SQL logical server: sql-servicedesk-neeraj-2026
- Server FQDN: sql-servicedesk-neeraj-2026.database.windows.net
- Port: 1433
- Application login: servicedesk_app

The actual password and full connection string must remain in Azure App Service configuration and must never be written into this repository.

### SQL Server migrations

The SQL Server EF Core migrations are stored in **Capstone.DAL.SqlServerMigrations**. Its design-time factory reads **AZURE_SQL_CONNECTION_STRING** from the local process environment when migration tooling is run. This is separate from the SQLite design-time factory in **Capstone.DAL**.

The Azure SQL schema was applied using the SQL Server migration project. The API config selects **Capstone.DAL.SqlServerMigrations** as its SQL Server migrations assembly.

## GitHub Actions CI/CD

### Backend: .github/workflows/azure-deploy.yml

The workflow:

1. Checks out the repository.
2. Installs the .NET 10 SDK.
3. Restores Capstone.Tests/Capstone.Tests.csproj.
4. Runs the automated test project using test-only JWT settings.
5. Publishes Capstone.API in Release configuration.
6. Uploads the published API as a workflow artifact.
7. Uses azure/login with OIDC to authenticate to Azure.
8. Deploys the artifact to the servicedesk-api-neeraj-2026 App Service.

The deployment job runs only for pushes to **main** after the build-and-test job succeeds. Pull requests can run build/tests but do not deploy.

The GitHub repository uses these encrypted Actions secrets:

- AZURE_CLIENT_ID
- AZURE_TENANT_ID
- AZURE_SUBSCRIPTION_ID

The workflow requests **id-token: write** only in the deployment job. Azure trusts the GitHub branch identity through a federated credential on the user-assigned managed identity. No long-lived Azure client secret or publish profile is needed.

### Frontend: .github/workflows/azure-static-web-apps-proud-stone-0019ac600.yml

The workflow uses the Azure Static Web Apps deployment action. Its build configuration is:

| Setting | Value |
|---|---|
| App location | ./Capstone.React |
| API location | Empty (API is hosted separately) |
| Output location | dist |
| Build command | npm run build (detected by Oryx from package.json) |
| Vite API variable | VITE_API_BASE_URL |

The frontend workflow injects the production API base URL during the Vite build. Vite environment variables prefixed with VITE_ are included in the browser bundle; they must never contain secrets. The Static Web Apps deployment token is stored in a GitHub Actions secret.

## Frontend routing

The React application uses BrowserRouter. The file **Capstone.React/public/staticwebapp.config.json** configures a navigation fallback to **/index.html**, allowing deep links and browser refreshes to be handled by React Router.

## Local build commands

From the repository root:

~~~bash
dotnet restore Capstone.Tests/Capstone.Tests.csproj
dotnet test Capstone.Tests/Capstone.Tests.csproj --configuration Release
dotnet publish Capstone.API/Capstone.API.csproj --configuration Release --output ./publish
~~~

For the frontend:

~~~bash
cd Capstone.React
npm ci
npm run build
~~~

The frontend build output is **Capstone.React/dist**.

## Deployment verification

The API login route is:

~~~text
POST https://servicedesk-api-neeraj-2026-fkgseadcemc2gpf0.centralindia-01.azurewebsites.net/api/ITSRPAPI/Login
~~~

Use a deliberate invalid test username/password for a smoke test. An HTTP 401 response demonstrates that the API route is responding and rejected the supplied credentials; it does not test successful authentication. Never put a real account password or JWT into a shell history, screenshot, issue, or public commit.

A successful live functional check should separately confirm:
- user registration returns success;
- login returns an access token;
- a user can create and view their own request;
- an Admin can review and resolve requests;
- eligible users can reopen a resolved request once;
- audit history reflects the expected actions;
- the resulting records are present in Azure SQL.

## F1 Free plan operational note

The API App Service currently uses the F1 Free tier. It has strict CPU and outbound data quotas, and it does not provide a production availability SLA. During deployment troubleshooting, the site entered **QuotaExceeded** state and returned 503 responses. After the quota state returned to **Normal**, the application was reachable again.

Useful checks from Azure Cloud Shell:

~~~bash
az webapp show \
  --name servicedesk-api-neeraj-2026 \
  --resource-group rg-servicedesk-portfolio \
  --query "{State:state, UsageState:usageState, Availability:availabilityState}" \
  --output yaml
~~~

Do not assume that all Azure services are free because one resource is on a Free tier. Review Cost Management, resource-specific quotas, and subscription offer terms regularly. If the hosted demo is unavailable, the GitHub source and local run instructions remain the reproducible fallback.

## Secret handling

Never commit or publish:
- the full SQL connection string or SQL application password;
- the Production JWT signing key;
- Admin seed credentials;
- the Azure Static Web Apps deployment token;
- GitHub Actions secret values, cookies, or access tokens.

Use Azure App Service environment settings for runtime secrets and GitHub Actions encrypted secrets for deployment authentication.

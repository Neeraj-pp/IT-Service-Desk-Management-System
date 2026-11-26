# Portfolio Evidence and Screenshot Index

The repository now contains screenshots of the original application UI, Azure deployment resources, GitHub Actions workflow runs, live application usage, and Azure SQL evidence.

All evidence images are stored directly in the existing **screenshots/** folder.

## Screenshot files currently in the repository

| File | Evidence represented |
|---|---|
| screenshots/01-login.png | Original login UI |
| screenshots/02-user-dashboard.png | Original User dashboard UI |
| screenshots/03-raise-request.png | Original raise-request UI |
| screenshots/04-admin-dashboard.png | Original Admin dashboard UI |
| screenshots/05-audit-history.png | Original audit history UI |
| screenshots/06-request-details.png | Original request details UI |
| screenshots/07-swagger-api.png | Development Swagger/API documentation |
| screenshots/08-azure-static-web-app-overview.png | Azure Static Web Apps resource overview |
| screenshots/09-azure-app-service-overview.png | Azure App Service overview |
| screenshots/10-azure-sql-database-overview.png | Azure SQL Database resource overview |
| screenshots/11-github-api-actions-success.png | Successful backend GitHub Actions run |
| screenshots/12-github-frontend-actions-success.png | Successful frontend GitHub Actions run |
| screenshots/13-live-user-registration.png | Live user registration evidence |
| screenshots/14-azure-sql-demo-records.png | Azure SQL demo record query results |
| screenshots/14-live-user-dashboard.png | Live User dashboard |
| screenshots/15-live-admin-dashboard.png | Live Admin dashboard |
| screenshots/16-azure-sql-schema-tables.png | Azure SQL schema and tables |

**Filename note:** Two uploaded files currently start with the numeric prefix 14. Their complete paths above are distinct. The README and Deployment Guide link to each exact filename, so neither file is ambiguous in the rendered documentation.

## What the CI/CD screenshots should prove

### Backend — screenshots/11-github-api-actions-success.png

Capture the successful run details for **Build and Deploy IT Service Desk API**. The run summary should show both jobs green:
- Build, Test and Publish API
- Deploy API to Azure App Service

Do not use the general **All workflows** page as the only backend proof, because it includes historical failed runs. Open the successful run itself.

### Frontend — screenshots/12-github-frontend-actions-success.png

Capture the successful run details for **Azure Static Web Apps CI/CD**. The run summary should show the **Build and Deploy Job** green. Open the successful run itself rather than relying only on the general workflow list.

## Security rules for public evidence

- Use demonstration usernames and synthetic request descriptions.
- Never show passwords, password hashes, JWTs, cookies, SQL connection strings/passwords, JWT signing keys, Static Web Apps deployment tokens, or GitHub secret values.
- Do not publish the GitHub Secrets values page.
- Hide subscription IDs, tenant IDs, personal email addresses, phone numbers, and billing details unless there is a specific reason to show them.
- Review each screenshot before committing it. Remove or replace any image that exposes credentials or personal information.

## Optional evidence still worth adding

The current screenshots already establish the Azure resources, both CI/CD workflows, live registration, both dashboards, SQL schema, and persisted demo records. Additional optional evidence could include:

- a live request creation screen using a synthetic request;
- the same request after Admin resolution;
- the same request after the owner reopens it;
- an audit history screen showing those events;
- a safe API response showing a successful test request (with tokens and credentials removed).

Only add these after performing the corresponding actions in the deployed application.

## Screenshot upload commands

From the local repository root:

~~~powershell
git add screenshots/
git status
git diff --cached --stat
~~~

Review the staged file list and inspect the screenshots before committing. Then:

~~~powershell
git commit -m "docs: add Azure deployment and live application evidence"
git push origin main
~~~

If the push is rejected because GitHub has newer commits, synchronize with **git pull --rebase origin main** and push again. Never use force-push for evidence uploads.

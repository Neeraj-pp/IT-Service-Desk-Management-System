# Portfolio Evidence Capture Checklist

This checklist gives the exact screenshots recommended for a recruiter-friendly portfolio. Capture real screenshots from the deployed application, Azure Portal, and GitHub Actions. Do not fabricate results.

Keep the existing seven screenshots in the repository root **screenshots/** folder. Add new evidence in two separate folders:

- **screenshots/production/** — live application and end-to-end user flows.
- **screenshots/deployment/** — Azure resources, GitHub Actions, and database proof.

## A. Existing application screenshots — keep these

These seven files already exist and are linked from the root README. Do not delete or rename them:

- \`screenshots/01-login.png\`
- \`screenshots/02-user-dashboard.png\`
- \`screenshots/03-raise-request.png\`
- \`screenshots/04-admin-dashboard.png\`
- \`screenshots/05-audit-history.png\`
- \`screenshots/06-request-details.png\`
- \`screenshots/07-swagger-api.png\`

They document the application UI. The new production screenshots below provide evidence that the hosted version works.

## B. Live application screenshots

Save these in **screenshots/production/**.

| Filename | Exactly what to show |
|---|---|
| \`01-live-login-page.png\` | The deployed React login page. Keep the browser address bar visible so the Static Web Apps hostname is visible. No password should be entered. |
| \`02-live-registration-success.png\` | Successful registration confirmation for a demonstration user. Do not show a password. |
| \`03-live-user-dashboard.png\` | Logged-in demonstration User dashboard with sample request counts or data. |
| \`04-live-raise-request.png\` | The User's raise-request form with a harmless sample description. Do not include personal information. |
| \`05-live-user-request-list.png\` | The User dashboard/list showing the newly created sample request and its status. |
| \`06-live-admin-dashboard.png\` | Logged-in Admin dashboard showing request summary/counts and the sample request. Do not show Admin credentials or tokens. |
| \`07-live-request-resolved.png\` | Admin view showing the sample request after it has been resolved. |
| \`08-live-request-reopened.png\` | User view showing the same eligible request after the owner reopens it. |
| \`09-live-audit-history.png\` | Admin audit history showing the create, resolve, and reopen events for the demonstration request. |

If you have not yet tested one of these steps, do not create a fake screenshot. Run the relevant workflow first, or leave that item unchecked.

## C. Azure and CI/CD screenshots

Save these in **screenshots/deployment/**.

| Filename | Exactly what to show |
|---|---|
| \`01-azure-resource-group-overview.png\` | Azure resource group resource list showing the Static Web App, App Service, SQL Server/database, and managed identity resources. Hide subscription ID and unrelated account details. |
| \`02-azure-static-web-app-overview.png\` | Static Web App Overview showing its resource name and default hostname. |
| \`03-azure-app-service-overview.png\` | App Service Overview showing the app name, Linux/.NET 10 runtime, and current Running state. Capture only when the app is healthy; do not imply guaranteed uptime. |
| \`04-azure-sql-database-overview.png\` | Azure SQL database Overview showing database name, server name, and service tier/compute configuration. Do not open or show connection strings or credentials. |
| \`05-github-api-actions-success.png\` | GitHub Actions backend workflow run showing both **Build, Test and Publish API** and **Deploy API to Azure App Service** jobs green. |
| \`06-github-frontend-actions-success.png\` | GitHub Actions Static Web Apps run showing the build/deploy job green. |
| \`07-azure-sql-schema-tables.png\` | Azure SQL Query Editor/Object Explorer showing the application tables and migration history table. Do not show credentials or connection properties. |
| \`08-azure-sql-demo-records.png\` | Query result showing a small number of synthetic/demo users and/or service requests persisted in Azure SQL. Avoid personal data and never show password hashes. |
| \`09-live-api-smoke-test.png\` | Terminal/Postman response for a safe API smoke test. A deliberate invalid login returning HTTP 401 is acceptable proof that the route is responding; label it as an invalid-credentials smoke test, not a successful login. Hide tokens, cookies, and request passwords. |

## D. Optional extra evidence

Capture these only if they add value:

- A GitHub workflow run showing a pull request runs build/tests without deploying.
- A screenshot of repository folders showing the API, DAL, SQL Server migrations, React, tests, and workflows.
- A screenshot of the README architecture diagram and live demo links.
- Azure Monitor/App Service metrics, if you want to document operational monitoring and free-tier quota limits.

## E. How to capture clean screenshots

1. Maximize the browser or relevant window.
2. Keep the important resource name, URL, workflow name, status, or table heading visible.
3. Crop out unrelated tabs and desktop clutter where possible.
4. Use synthetic demonstration usernames and request descriptions.
5. For live application screens, use separate demo User and Admin accounts.
6. Capture the full result so the evidence is understandable without additional explanation.
7. Use PNG format where possible.

## F. Security checklist before saving or committing

**Never include**:
- passwords (including SQL app-user or Admin passwords);
- JWT access/refresh tokens or cookies;
- SQL connection strings or SQL passwords;
- JWT signing keys;
- Static Web Apps deployment tokens;
- GitHub Actions secret values;
- password hashes from the Users table;
- private personal information.

Do not capture the GitHub Secrets values page. A screenshot showing only secret names is not necessary. Do not expose subscription IDs, tenant IDs, public IP addresses, or billing details unless there is a specific reason.

## G. Add the screenshots to GitHub

Create the two folders locally if they do not exist:

~~~powershell
cd C:\Resume_Project\CapstoneProject_Upgrade\CapstoneProject

New-Item -ItemType Directory -Force screenshots\production
New-Item -ItemType Directory -Force screenshots\deployment
~~~

Save the screenshots using the exact filenames in sections B and C. Then review the staged files:

~~~powershell
git add screenshots/production/
git add screenshots/deployment/
git status
git diff --cached --stat
~~~

Open the staged file list and make sure it contains only the intended PNG evidence. Then commit and push:

~~~powershell
git commit -m "docs: add live application and Azure deployment evidence"
git push origin main
~~~

If the push is rejected because GitHub has newer commits, do not force-push. Synchronize safely with \`git pull --rebase origin main\`, resolve any conflicts if prompted, then push again.

After uploading the evidence, update the root README to link or embed selected screenshots from **screenshots/production/** and **screenshots/deployment/**.

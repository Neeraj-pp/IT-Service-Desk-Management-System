# Portfolio Evidence Capture Checklist

Save the evidence screenshots directly in the existing repository **screenshots/** folder. Keep the seven existing application screenshots unchanged.

Capture genuine screenshots from your own app, Azure Portal, and GitHub Actions. Do not fabricate results or include credentials.

## Existing application screenshots — keep these

- `screenshots/01-login.png`
- `screenshots/02-user-dashboard.png`
- `screenshots/03-raise-request.png`
- `screenshots/04-admin-dashboard.png`
- `screenshots/05-audit-history.png`
- `screenshots/06-request-details.png`
- `screenshots/07-swagger-api.png`

## New Azure and CI/CD evidence — save in screenshots/

| Filename | Capture |
|---|---|
| `08-azure-resource-group-overview.png` | Azure resource group resource list for this project. Hide subscription IDs and unrelated account information. |
| `09-azure-static-web-app-overview.png` | Static Web App Overview showing resource name and default hostname. |
| `10-azure-app-service-overview.png` | App Service Overview showing .NET 10 runtime and current Running state. |
| `11-github-api-actions-success.png` | **Backend workflow run details** showing both Build, Test and Publish API and Deploy API to Azure App Service jobs green. |
| `12-github-frontend-actions-success.png` | **Frontend workflow run details** showing the Azure Static Web Apps Build and Deploy Job green. |
| `13-azure-sql-schema-tables.png` | Azure SQL Object Explorer/Query Editor showing the application tables and migration history table. |
| `14-azure-sql-demo-records.png` | Query results with synthetic/demo users or service requests. Never show password hashes or personal data. |

## New live application evidence — save in screenshots/

| Filename | Capture |
|---|---|
| `15-live-registration-success.png` | Successful registration confirmation for a demonstration account. Do not show a password. |
| `16-live-user-dashboard.png` | Live User dashboard with synthetic/demo data. |
| `17-live-admin-dashboard.png` | Live Admin dashboard with synthetic/demo data. |
| `18-live-request-lifecycle-audit.png` | Evidence of a sample request being created, resolved, reopened, and recorded in audit history. Capture actual screens; this may require more than one screenshot if one screen cannot show the whole lifecycle. |
| `19-live-api-smoke-test.png` | Safe API smoke-test response. An invalid-login HTTP 401 can prove the endpoint responds, but label it as an invalid-credentials smoke test, not successful authentication. |

## GitHub Actions: exactly which screen to capture

### Backend — `11-github-api-actions-success.png`

1. Open the repository's **Actions** tab.
2. In the left workflow list, select **Build and Deploy IT Service Desk API**.
3. Select the latest successful run titled **Improve CI/CD workflow and configure test JWT settings** (commit `836ccf9` in the current run history).
4. Capture the run summary page where both jobs are green:
   - **Build, Test and Publish API**
   - **Deploy API to Azure App Service**
5. Prefer a screenshot with the workflow title, main branch, commit, and both green job boxes visible.

### Frontend — `12-github-frontend-actions-success.png`

1. Open the repository's **Actions** tab.
2. In the left workflow list, select **Azure Static Web Apps CI/CD**.
3. Select the latest successful run. In the current run history, the successful run titled **docs: expand exact production and Azure evidence checklist** (commit `4c8ab11`) is suitable.
4. Capture the run summary page where **Build and Deploy Job** is green.
5. Prefer a screenshot with the workflow title, main branch, commit, and green job box visible.

**Do not use the general All workflows list as either of these two screenshots.** It includes earlier failed runs as well as successful runs, and does not prove the individual workflow stages as clearly. Open each successful run separately.

## Screenshot safety rules

- Use synthetic demonstration usernames and request descriptions.
- Never show passwords, password hashes, JWTs, cookies, SQL connection strings/passwords, JWT signing keys, Static Web Apps deployment tokens, or GitHub secret values.
- Do not capture the GitHub Secrets values page.
- Hide subscription IDs, tenant IDs, personal email addresses, phone numbers, and billing details unless there is a specific reason to show them.
- Crop unrelated browser tabs and desktop clutter where practical.
- Only capture results that actually occurred. Leave a checklist item unfinished if you have not performed that test.

## Add the screenshots to Git

From the local repository root:

```powershell
git add screenshots/
git status
git diff --cached --stat
```

Review the staged file list and images. Confirm that there are no credentials or personal data. Then:

```powershell
git commit -m "docs: add Azure deployment and live application evidence"
git push origin main
```

If the push is rejected because GitHub has newer commits, synchronize with `git pull --rebase origin main` and push again. Never use force-push for this evidence upload.

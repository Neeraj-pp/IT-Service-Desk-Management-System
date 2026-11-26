# Portfolio Evidence Capture Checklist

Use this checklist to add genuine deployment and testing evidence to the repository. Capture screenshots from your own Azure Portal, GitHub Actions, and application. Do not fabricate results or include credentials.

Recommended destination: **screenshots/deployment/**.

## Already in the repository

The following application UI screenshots are already present:

- 01-login.png
- 02-user-dashboard.png
- 03-raise-request.png
- 04-admin-dashboard.png
- 05-audit-history.png
- 06-request-details.png
- 07-swagger-api.png

## Add deployment proof

Capture the following after removing passwords, keys, connection strings, subscription IDs where not needed, personal data, and tokens from the visible screen.

| Suggested filename | Evidence to capture |
|---|---|
| 08-azure-static-web-app-overview.png | Static Web App resource overview showing resource name and deployment hostname |
| 09-azure-app-service-overview.png | App Service overview showing .NET 10 runtime and current running status |
| 10-azure-sql-database-overview.png | Azure SQL database overview showing database resource and service tier (hide server credentials) |
| 11-github-api-actions-success.png | Successful backend build/test/deploy GitHub Actions run with both jobs green |
| 12-github-frontend-actions-success.png | Successful Static Web Apps build/deploy run |
| 13-live-user-registration.png | Successful registration confirmation using a demonstration account |
| 14-live-user-dashboard.png | Deployed user dashboard with synthetic/demo data |
| 15-live-admin-dashboard.png | Deployed Admin dashboard with synthetic/demo data |
| 16-azure-sql-data-proof.png | Azure SQL query result showing expected demo rows (no personal or secret data) |

## Add test proof

When suitable screenshots are available, consider documenting:
- registration success;
- valid login and role assignment;
- unauthorized request without a token;
- forbidden access for a role that lacks permission;
- request creation and status lifecycle;
- audit records after request actions;
- database persistence after refresh/relogin.

## Screenshot safety rules

- Use demonstration usernames and synthetic request descriptions.
- Never show a password, JWT, cookie, SQL connection string, SQL password, JWT signing key, Static Web Apps deployment token, or GitHub secret value.
- Avoid showing personal email addresses, phone numbers, tenant identifiers, subscription IDs, and billing details unless genuinely needed.
- Crop browser tabs and unrelated account details when practical.
- Do not add a screenshot if it does not prove the stated result.

## Add screenshots to Git

From the local repository root:

~~~powershell
git add screenshots/deployment/
git status
git diff --cached --stat
git commit -m "docs: add Azure deployment evidence"
git push origin main
~~~

Review the staged files before committing. Confirm no secrets or real user data are included.

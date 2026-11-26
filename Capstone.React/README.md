# Capstone.React — IT Service Desk frontend

This folder contains the React single-page application for the IT Service Desk Management System. It is the user-facing portal for standard users and administrators.

## Technology

- React 19
- Vite 7
- React Router 7
- Axios for API communication
- JavaScript and CSS

## Features implemented

### Public authentication
- Login using the backend authentication API.
- Self-service user registration.
- The backend assigns the standard `User` role during public registration; role selection is not exposed as a client-side privilege.
- On successful login, the application stores the returned access token and expiry information for the active browser session.

### Authenticated application
- Protected routes require an authenticated session.
- Role-aware routing separates the User and Admin experiences.
- The Axios client attaches a Bearer token to requests while the session is valid.
- Expired or invalid sessions are cleared and redirected to login when the API responds with HTTP 401.
- HTTP 403 is handled as an authorization denial rather than an automatic logout.

### User experience
- View the User dashboard and own service requests.
- Raise service requests.
- View request details.
- Reopen eligible resolved requests, subject to backend ownership and lifecycle rules.
- Delete requests where permitted by the backend authorization rules.

### Admin experience
- View and manage service requests.
- Search/filter service requests.
- Resolve requests.
- View audit history and activity summaries.

The backend remains the authority for permissions, ownership, and request status transitions. Hiding a page or button in React is not treated as a security boundary.

## Source layout

```text
src/
├── components/
│   └── ProtectedRoute.jsx
├── pages/
│   ├── Login.jsx
│   ├── Register.jsx
│   ├── UserDashboard.jsx
│   ├── AdminDashboard.jsx
│   ├── AddRequest.jsx
│   ├── ReopenRequest.jsx
│   ├── DeleteRequest.jsx
│   ├── SearchRequest.jsx
│   └── AuditHistory.jsx
├── services/
│   ├── api.js
│   └── auth.jsx
├── styles/
│   └── main.css
├── App.jsx
└── main.jsx
public/
└── staticwebapp.config.json
```

## Environment configuration

The API base URL is read from the Vite variable `VITE_API_BASE_URL`. A local example is provided in `.env.example`:

```env
VITE_API_BASE_URL=https://localhost:5193/api
```

Copy the example to `.env.local` and change the URL to match the local ASP.NET Core API launch URL. Do not put secrets, API keys, passwords, or private connection strings in Vite environment variables: values prefixed with `VITE_` are bundled into browser-visible frontend code.

For the Azure Static Web Apps deployment, the GitHub Actions workflow injects the production API base URL at build time. The Static Web Apps navigation fallback in `public/staticwebapp.config.json` supports direct navigation and refreshes for React Router routes.

## Run locally

Requirements: Node.js and npm, plus the backend API running locally.

```bash
npm ci
npm run dev
```

Vite prints the local development URL in the terminal.

## Build and lint

```bash
npm run build
npm run lint
npm run preview
```

- `npm run build` creates the production bundle in `dist/`.
- `npm run lint` runs ESLint.
- `npm run preview` serves the built bundle locally for a production-build check.

## Deployment

The repository-root deployment guide documents the Azure Static Web Apps resource, production API URL, GitHub Actions workflow, and deployment proof:

- [Azure deployment guide](../docs/DEPLOYMENT.md)
- [Screenshot and portfolio evidence index](../docs/PROOF-CAPTURE-CHECKLIST.md)
- [Repository root README](../README.md)

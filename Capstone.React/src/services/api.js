import axios from "axios";
import { logout } from "./auth";

const API_BASE_URL =
    import.meta.env.VITE_API_BASE_URL ||
    "https://localhost:5193/api";

const API = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        "Content-Type": "application/json"
    }
});

// Attach JWT only when the session is valid.
API.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem("accessToken");
        const expiresAt = localStorage.getItem("tokenExpiresAt");

        if (!token) {
            return config;
        }

        const expiryTime = Date.parse(expiresAt || "");

        // Reject the request if the session has expired or its
        // expiry information is missing or invalid.
        if (!Number.isFinite(expiryTime) || expiryTime <= Date.now()) {
            logout();

            if (
                window.location.pathname !== "/" &&
                window.location.pathname !== "/register"
            ) {
                window.location.replace("/");
            }

            return Promise.reject(
                new Error("Your session has expired. Please sign in again.")
            );
        }

        config.headers.Authorization = `Bearer ${token}`;

        return config;
    },
    (error) => Promise.reject(error)
);

// Handle invalid or expired JWT responses.
API.interceptors.response.use(
    (response) => response,
    (error) => {
        if (
            error.response?.status === 401 &&
            localStorage.getItem("accessToken")
        ) {
            logout();

            if (
                window.location.pathname !== "/" &&
                window.location.pathname !== "/register"
            ) {
                window.location.replace("/");
            }
        }

        // HTTP 403 is deliberately not treated as logout.
        // It means the user is authenticated but lacks permission.
        return Promise.reject(error);
    }
);

export default API;
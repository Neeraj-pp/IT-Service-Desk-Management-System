
import { Navigate, Outlet } from "react-router-dom";

function ProtectedRoute({ allowedRoles }) {
    const token = localStorage.getItem("accessToken");
    const savedUser = localStorage.getItem("user");
    const expiresAt = localStorage.getItem("tokenExpiresAt");

    // Reject sessions with missing authentication details.
    if (!token || !savedUser || !expiresAt) {
        return <Navigate to="/" replace />;
    }

    // Validate saved user information.
    let user;

    try {
        user = JSON.parse(savedUser);
    } catch {
        return <Navigate to="/" replace />;
    }

    // Validate user role.
    const userRole = user?.role?.roleName;

    if (!userRole || !allowedRoles?.includes(userRole)) {
        return <Navigate to="/" replace />;
    }

    // API interceptor and backend enforce token expiration.
    return <Outlet />;
}

export default ProtectedRoute;

import { Routes, Route } from "react-router-dom";

import Login from "./pages/Login";
import Register from "./pages/Register";
import UserDashboard from "./pages/UserDashboard";
import AddRequest from "./pages/AddRequest";
import ReopenRequest from "./pages/ReopenRequest";
import AdminDashboard from "./pages/AdminDashboard";
import SearchRequest from "./pages/SearchRequest";
import DeleteRequest from "./pages/DeleteRequest";
import ProtectedRoute from "./components/ProtectedRoute";
import AuditHistory from "./pages/AuditHistory";

function App() {
    return (
        <Routes>
            {/* Public authentication routes */}
            <Route path="/" element={<Login />} />
            <Route path="/register" element={<Register />} />

            {/* Routes accessible to both Admin and User */}
            <Route element={<ProtectedRoute allowedRoles={["Admin", "User"]} />}>
                <Route path="/add-request" element={<AddRequest />} />
                <Route path="/reopen/:id" element={<ReopenRequest />} />
                <Route path="/delete/:id" element={<DeleteRequest />} />
            </Route>

            {/* User-only routes */}
            <Route element={<ProtectedRoute allowedRoles={["User"]} />}>
                <Route path="/user/dashboard" element={<UserDashboard />} />
            </Route>

            {/* Admin-only routes */}
            <Route element={<ProtectedRoute allowedRoles={["Admin"]} />}>
                <Route path="/admin/dashboard" element={<AdminDashboard />} />
                <Route path="/search" element={<SearchRequest />} />
                <Route path="/admin/audit-history" element={<AuditHistory />} />
            </Route>
        </Routes>
    );
}

export default App;

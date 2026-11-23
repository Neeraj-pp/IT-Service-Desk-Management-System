
import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import API from "../services/api";
import { logout } from "../services/auth";

function UserDashboard() {
    const [requests, setRequests] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");
    const [selectedRequest, setSelectedRequest] = useState(null);

    const navigate = useNavigate();

    let user = null;

    try {
        user = JSON.parse(localStorage.getItem("user"));
    } catch {
        user = null;
    }

    const handleLogout = () => {
        logout();
        navigate("/", { replace: true });
    };

    useEffect(() => {
        const loggedInUser = (() => {
            try {
                return JSON.parse(localStorage.getItem("user"));
            } catch {
                return null;
            }
        })();

        if (!loggedInUser || loggedInUser.role?.roleName !== "User") {
            navigate("/", { replace: true });
            return;
        }

        const fetchRequests = async () => {
            try {
                setLoading(true);
                setError("");

                const response = await API.get(
                    "/ITSRPAPI/GetRequestByuserName"
                );

                setRequests(
                    Array.isArray(response.data) ? response.data : []
                );
            } catch (error) {
                console.error("Error fetching requests:", error);
                setError("Unable to load your service requests.");
            } finally {
                setLoading(false);
            }
        };

        fetchRequests();
    }, [navigate]);

    if (!user || user.role?.roleName !== "User") {
        return null;
    }

    const getStatusName = (statusId) => {
        switch (statusId) {
            case 1:
                return "New";
            case 2:
                return "Resolved";
            case 3:
                return "Reopened";
            default:
                return "Unknown";
        }
    };

    const getStatusClass = (statusId) => {
        switch (statusId) {
            case 1:
                return "status-open";
            case 2:
                return "status-resolved";
            case 3:
                return "status-reopened";
            default:
                return "";
        }
    };

    return (
        <>
            <div className="navbar">
                <h2 className="logo">IT Service Portal</h2>

                <div className="nav-actions">
                    <Link to="/add-request" className="btn-primary">
                        Raise New Request
                    </Link>

                    <button onClick={handleLogout} className="btn-danger">
                        Logout
                    </button>
                </div>
            </div>

            <div className="content-wrapper">
                <h1 className="page-title">
                    Welcome {user.userName}
                </h1>

                {loading && <p>Loading your requests...</p>}

                {error && <p className="error">{error}</p>}

                {!loading && !error && requests.length === 0 && (
                    <p>You haven't raised any service requests yet.</p>
                )}

                {!loading && !error && requests.length > 0 && (
                    <table
                        border="1"
                        cellPadding="10"
                        style={{ marginTop: "20px" }}
                    >
                        <thead>
                            <tr>
                                <th>S.No.</th>
                                <th>Description</th>
                                <th>Creation Date</th>
                                <th>Request Status</th>
                                <th>Actions</th>
                            </tr>
                        </thead>

                        <tbody>
                            {requests.map((req, index) => (
                                <tr key={req.requestId}>
                                    <td>{index + 1}</td>
                                    <td>{req.description}</td>
                                    <td>
                                        {new Date(req.raisedOn).toLocaleString()}
                                    </td>
                                    <td>
                                        <span className={getStatusClass(req.reqStatus)}>
                                            {getStatusName(req.reqStatus)}
                                        </span>
                                    </td>

                                    <td>
                                        <button
                                            className="user-view-btn"
                                            onClick={() => setSelectedRequest(req)}
                                        >
                                            View
                                        </button>

                                        {req.reqStatus === 2 && (
                                            <Link
                                                to={`/reopen/${req.requestId}`}
                                                className="btn-reopen"
                                            >
                                                Reopen
                                            </Link>
                                        )}

                                        <Link
                                            to={`/delete/${req.requestId}`}
                                            className="btn-delete"
                                        >
                                            Delete
                                        </Link>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                )}
            </div>

            {selectedRequest && (
                <div
                    className="user-modal-overlay"
                    onClick={() => setSelectedRequest(null)}
                >
                    <div
                        className="user-modal-content"
                        onClick={(e) => e.stopPropagation()}
                    >
                        <div className="user-modal-header">
                            <div>
                                <span className="user-modal-eyebrow">
                                    SERVICE REQUEST DETAILS
                                </span>
                                <h2>Request Details</h2>
                                <p>
                                    Review the information submitted with your request.
                                </p>
                            </div>

                            <button
                                className="user-modal-close"
                                onClick={() => setSelectedRequest(null)}
                                aria-label="Close"
                            >
                                ×
                            </button>
                        </div>

                        <div className="user-modal-body">
                            <div className="user-modal-status-card">
                                <div>
                                    <span>Current Request Status</span>
                                    <strong className={getStatusClass(selectedRequest.reqStatus)}>
                                        {getStatusName(selectedRequest.reqStatus)}
                                    </strong>
                                </div>
                            </div>

                            <h3>Request Information</h3>

                            <div className="user-modal-info-card">
                                <span>Created On</span>
                                <strong>
                                    {new Date(
                                        selectedRequest.raisedOn
                                    ).toLocaleString()}
                                </strong>
                            </div>

                            <div className="user-modal-section">
                                <h3>
                                    <span>01</span>
                                    Request Description
                                </h3>
                                <div className="user-modal-text">
                                    {selectedRequest.description || "No description available"}
                                </div>
                            </div>

                            <div className="user-modal-section">
                                <h3>
                                    <span>02</span>
                                    Detailed Explanation
                                </h3>
                                <div className="user-modal-text">
                                    {selectedRequest.details || "No additional details available"}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </>
    );
}

export default UserDashboard;

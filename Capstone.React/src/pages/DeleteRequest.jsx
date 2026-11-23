
import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import API from "../services/api";

function DeleteRequest() {
    const { id } = useParams();
    const navigate = useNavigate();

    const [request, setRequest] = useState(null);
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(true);
    const [deleting, setDeleting] = useState(false);

    // Get logged-in user
    const user = (() => {
        try {
            return JSON.parse(localStorage.getItem("user"));
        } catch {
            return null;
        }
    })();

    // Determine dashboard based on role
    const dashboardPath =
        user?.role?.roleName === "Admin"
            ? "/admin/dashboard"
            : "/user/dashboard";

    // Load request details
    useEffect(() => {
        const loadRequest = async () => {
            if (!user || !["Admin", "User"].includes(user.role?.roleName)) {
                navigate("/", { replace: true });
                return;
            }

            if (!id || !/^\d+$/.test(id) || Number(id) <= 0) {
                setError("Invalid request ID.");
                setLoading(false);
                return;
            }

            try {
                setLoading(true);
                setError("");

                const response = await API.get(
                    `/ITSRPAPI/GetRequestById?reqId=${id}`
                );

                setRequest(response.data);

            } catch (error) {
                console.error("Error loading request:", error);

                if (error.response?.status === 404) {
                    setError("Request not found.");
                } else if (error.response?.status === 403) {
                    setError("You are not authorized to access this request.");
                } else {
                    setError("Unable to load the request. Please try again.");
                }
            } finally {
                setLoading(false);
            }
        };

        loadRequest();
    }, [navigate, id]);

    // Delete request
    const deleteRequest = async () => {
        if (!request || deleting) {
            return;
        }

        const confirmed = window.confirm(
            `Are you sure you want to permanently delete request #${id}?`
        );

        if (!confirmed) {
            return;
        }

        try {
            setDeleting(true);
            setError("");

            await API.delete(`/ITSRPAPI/Delete/${id}`);

            alert("Request deleted successfully.");

            navigate(dashboardPath, { replace: true });

        } catch (error) {
            console.error("Error deleting request:", error);

            if (error.response?.status === 404) {
                setError("Request not found or it has already been deleted.");
            } else if (error.response?.status === 403) {
                setError("You are not authorized to delete this request.");
            } else if (error.response?.status === 401) {
                setError("Your session has expired. Please login again.");
            } else {
                setError("Unable to delete the request. Please try again.");
            }
        } finally {
            setDeleting(false);
        }
    };

    if (!user || !["Admin", "User"].includes(user.role?.roleName)) {
        return null;
    }

    if (loading) {
        return (
            <div className="container">
                <h2>Loading request...</h2>
            </div>
        );
    }

    if (!request) {
        return (
            <div className="container">
                <h2>Delete Request</h2>

                <p className="error">
                    {error || "Request not found."}
                </p>

                <button
                    onClick={() => navigate(dashboardPath)}
                    className="btn btn-secondary"
                >
                    Back to Dashboard
                </button>
            </div>
        );
    }

    return (
        <div className="container">
            <h2>Delete Service Request</h2>

            <p>
                Are you sure you want to permanently delete this request?
            </p>

            <div className="table-container">
                <table
                    border="1"
                    cellPadding="10"
                    style={{ marginTop: "20px" }}
                >
                    <thead>
                        <tr>
                            <th>Request ID</th>
                            <th>Description</th>
                            <th>Details</th>
                            <th>Requested By</th>
                            <th>Creation Date</th>
                        </tr>
                    </thead>

                    <tbody>
                        <tr>
                            <td>{request.requestId}</td>
                            <td>{request.description}</td>
                            <td>{request.details}</td>
                            <td>{request.raisedBy}</td>
                            <td>
                                {new Date(request.raisedOn).toLocaleString()}
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>

            {error && (
                <p className="error">{error}</p>
            )}

            <button
                onClick={deleteRequest}
                className="btn btn-danger"
                disabled={deleting}
            >
                {deleting ? "Deleting..." : "Delete Request"}
            </button>

            <br />
            <br />

            <button
                onClick={() => navigate(dashboardPath)}
                className="btn btn-secondary"
                disabled={deleting}
            >
                Back
            </button>
        </div>
    );
}

export default DeleteRequest;

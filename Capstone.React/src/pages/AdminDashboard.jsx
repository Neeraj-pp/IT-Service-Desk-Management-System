
import { useCallback, useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import API from "../services/api";
import { logout } from "../services/auth";

function AdminDashboard() {
    const [requests, setRequests] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    const [selectedRequest, setSelectedRequest] = useState(null);

    // Dashboard summary counts
    const [summary, setSummary] = useState({
        total: 0,
        newRequests: 0,
        reopened: 0,
        closed: 0
    });

    const [refreshing, setRefreshing] = useState(false);

    // Pagination
    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [totalRecords, setTotalRecords] = useState(0);
    const [totalPages, setTotalPages] = useState(0);

    // Filter input values
    const [filters, setFilters] = useState({
        searchTerm: "",
        statusId: "",
        fromDate: "",
        toDate: ""
    });

    // Filters currently applied to the table
    const [appliedFilters, setAppliedFilters] = useState({
        searchTerm: "",
        statusId: "",
        fromDate: "",
        toDate: ""
    });

    const navigate = useNavigate();

    const admin = (() => {
        try {
            return JSON.parse(localStorage.getItem("user"));
        } catch {
            return null;
        }
    })();

    const handleLogout = () => {
        logout();
        navigate("/", { replace: true });
    };

    // Load paginated requests from backend
    const loadRequests = useCallback(async (showLoading = true) => {
        try {
            if (showLoading) {
                setLoading(true);
            }

            setError("");

            const response = await API.get(
                "/ITSRPAPI/GetAllRequest",
                {
                    params: {
                        pageNumber,
                        pageSize,
                        searchTerm:
                            appliedFilters.searchTerm || undefined,
                        statusId: appliedFilters.statusId
                            ? Number(appliedFilters.statusId)
                            : undefined,
                        fromDate:
                            appliedFilters.fromDate || undefined,
                        toDate:
                            appliedFilters.toDate || undefined
                    }
                }
            );

            setRequests(response.data.items || []);
            setTotalRecords(response.data.totalRecords);
            setTotalPages(response.data.totalPages);
        } catch (error) {
            console.error("Error loading requests:", error);
            setError("Unable to load service requests.");
        } finally {
            if (showLoading) {
                setLoading(false);
            }
        }
    }, [pageNumber, pageSize, appliedFilters]);

    // Load all request status counts
    const loadSummary = useCallback(async () => {
        try {
            const response = await API.get(
                "/ITSRPAPI/GetRequestStatusCounts"
            );

            setSummary({
                total: response.data.total,
                newRequests: response.data.newRequests,
                reopened: response.data.reopened,
                closed: response.data.closed
            });
        } catch (error) {
            console.error("Error loading request summary:", error);
        }
    }, []);

    // Refresh both table and summary
    const refreshDashboard = useCallback(async (showLoading = true) => {
        setRefreshing(true);

        try {
            await Promise.all([
                loadRequests(showLoading),
                loadSummary()
            ]);
        } finally {
            setRefreshing(false);
        }
    }, [loadRequests, loadSummary]);

    // Close a service request
    const closeRequest = async (id) => {
        try {
            await API.put(`/ITSRPAPI/CloseRequest/${id}`);

            alert("Request marked as resolved successfully.");

            // Refresh request table and summary counts
            await refreshDashboard(false);

            // Update selected request if its details are open
            setSelectedRequest((current) =>
                current?.requestId === id
                    ? { ...current, reqStatus: 2 }
                    : current
            );
        } catch (error) {
            console.error("Error closing request:", error);

            alert(
                error.response?.data?.message ||
                "Unable to resolve the request."
            );
        }
    };

    // Initial load and reload when page or filters change
    useEffect(() => {
        if (!admin || admin.role?.roleName !== "Admin") {
            navigate("/", { replace: true });
            return;
        }

        refreshDashboard();
    }, [navigate, refreshDashboard]);

    // Automatically refresh every 15 seconds
    useEffect(() => {
        const interval = setInterval(() => {
            refreshDashboard(false);
        }, 15000);

        return () => clearInterval(interval);
    }, [refreshDashboard]);

    // Close modal using Escape key
    useEffect(() => {
        const handleKeyDown = (event) => {
            if (event.key === "Escape") {
                setSelectedRequest(null);
            }
        };

        window.addEventListener("keydown", handleKeyDown);

        return () => {
            window.removeEventListener("keydown", handleKeyDown);
        };
    }, []);

    if (!admin || admin.role?.roleName !== "Admin") {
        return null;
    }

    // Convert status IDs into readable labels
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

    // Get status CSS class
    const getStatusClass = (statusId) => {
        switch (statusId) {
            case 1:
                return "status-open";
            case 2:
                return "status-closed";
            case 3:
                return "status-reopened";
            default:
                return "";
        }
    };

    // Format date safely
    const formatDate = (date) => {
        if (!date) return "Not available";

        const parsedDate = new Date(date);

        if (Number.isNaN(parsedDate.getTime())) {
            return "Not available";
        }

        return parsedDate.toLocaleString();
    };

    // Apply a status filter when a summary card is clicked
    const handleSummaryCardClick = (statusId) => {
        setFilters((current) => ({
            ...current,
            statusId
        }));

        setPageNumber(1);

        setAppliedFilters((current) => ({
            ...current,
            statusId
        }));
    };

    return (
        <>
            {/* NAVBAR */}
            <div className="navbar">
                <h2 className="logo">
                    IT Service Portal
                </h2>

                <div className="nav-actions">
                    <Link
                        to="/search"
                        className="btn-secondary"
                    >
                        Search Requests
                    </Link>

                    <Link
                        to="/admin/audit-history"
                        className="btn-secondary"
                    >
                        Audit History
                    </Link>

                    <button
                        onClick={handleLogout}
                        className="btn-danger"
                    >
                        Logout
                    </button>
                </div>
            </div>

            {/* MAIN CONTENT */}
            <div className="content-wrapper">

                <h1 className="page-title">
                    Welcome {admin.userName}
                </h1>

                <p className="admin-dashboard-subtitle">
                    Manage, review and track all service requests.
                    Select a request to view its complete information.
                </p>

                {/* REQUEST SUMMARY CARDS */}
                <div className="request-summary-grid">

                    <button
                        type="button"
                        className={`summary-card summary-total ${appliedFilters.statusId === ""
                                ? "summary-active"
                                : ""
                            }`}
                        onClick={() => handleSummaryCardClick("")}
                    >
                        <span>Total Requests</span>
                        <strong>{summary.total}</strong>
                    </button>

                    <button
                        type="button"
                        className={`summary-card summary-new ${appliedFilters.statusId === "1"
                                ? "summary-active"
                                : ""
                            }`}
                        onClick={() => handleSummaryCardClick("1")}
                    >
                        <span>New Requests</span>
                        <strong>{summary.newRequests}</strong>
                    </button>

                    <button
                        type="button"
                        className={`summary-card summary-reopened ${appliedFilters.statusId === "3"
                                ? "summary-active"
                                : ""
                            }`}
                        onClick={() => handleSummaryCardClick("3")}
                    >
                        <span>Reopened Requests</span>
                        <strong>{summary.reopened}</strong>
                    </button>

                    <button
                        type="button"
                        className={`summary-card summary-closed ${appliedFilters.statusId === "2"
                                ? "summary-active"
                                : ""
                            }`}
                        onClick={() => handleSummaryCardClick("2")}
                    >
                        <span>Resolved Requests</span>
                        <strong>{summary.closed}</strong>
                    </button>

                </div>

                {/* REFRESH CONTROL */}
                <div className="dashboard-refresh-row">
                    <span>
                        Automatically refreshes every 15 seconds
                    </span>

                    <button
                        type="button"
                        className="btn-refresh"
                        onClick={() => refreshDashboard(false)}
                        disabled={refreshing}
                    >
                        {refreshing ? "Refreshing..." : "↻ Refresh"}
                    </button>
                </div>

                {/* FILTER SECTION */}
                <div className="filter-container">
                    <form
                        onSubmit={(e) => {
                            e.preventDefault();

                            if (
                                filters.fromDate &&
                                filters.toDate &&
                                filters.fromDate > filters.toDate
                            ) {
                                setError(
                                    "From date cannot be after To date."
                                );
                                return;
                            }

                            setError("");
                            setPageNumber(1);
                            setAppliedFilters({ ...filters });
                        }}
                    >
                        <input
                            type="text"
                            placeholder="Search description or details"
                            value={filters.searchTerm}
                            onChange={(e) =>
                                setFilters({
                                    ...filters,
                                    searchTerm: e.target.value
                                })
                            }
                        />

                        <select
                            value={filters.statusId}
                            onChange={(e) =>
                                setFilters({
                                    ...filters,
                                    statusId: e.target.value
                                })
                            }
                        >
                            <option value="">All Statuses</option>
                            <option value="1">New</option>
                            <option value="2">Resolved</option>
                            <option value="3">Reopened</option>
                        </select>

                        <label>From Date</label>

                        <input
                            type="date"
                            value={filters.fromDate}
                            onChange={(e) =>
                                setFilters({
                                    ...filters,
                                    fromDate: e.target.value
                                })
                            }
                        />

                        <label>To Date</label>

                        <input
                            type="date"
                            value={filters.toDate}
                            onChange={(e) =>
                                setFilters({
                                    ...filters,
                                    toDate: e.target.value
                                })
                            }
                        />

                        <button type="submit">
                            Apply Filters
                        </button>

                        <button
                            type="button"
                            onClick={() => {
                                const emptyFilters = {
                                    searchTerm: "",
                                    statusId: "",
                                    fromDate: "",
                                    toDate: ""
                                };

                                setFilters(emptyFilters);
                                setPageNumber(1);
                                setAppliedFilters(emptyFilters);
                                setError("");
                            }}
                        >
                            Clear Filters
                        </button>
                    </form>
                </div>

                {/* REQUEST TABLE */}
                <div className="table-container">

                    {loading && (
                        <p>Loading service requests...</p>
                    )}

                    {error && (
                        <p className="error">
                            {String(error)}
                        </p>
                    )}

                    {!loading && !error && requests.length === 0 && (
                        <p>No service requests found.</p>
                    )}

                    {!loading && !error && requests.length > 0 && (
                        <table
                            className="admin-requests-table"
                            border="1"
                            cellPadding="10"
                        >
                            <thead>
                                <tr>
                                    <th>S.No.</th>
                                    <th>Request ID</th>
                                    <th>Description</th>
                                    <th>Requested By</th>
                                    <th>Creation Date</th>
                                    <th>Request Status</th>
                                    <th>Action</th>
                                </tr>
                            </thead>

                            <tbody>
                                {requests.map((req, index) => (
                                    <tr
                                        key={req.requestId}
                                        className="admin-request-row"
                                        onClick={() =>
                                            setSelectedRequest(req)
                                        }
                                        title="Click to view request details"
                                    >
                                        <td>
                                            {(pageNumber - 1) * pageSize + index + 1}
                                        </td>

                                        <td>
                                            #{req.requestId}
                                        </td>

                                        <td>
                                            {req.description}
                                        </td>

                                        <td>
                                            {req.raisedBy}
                                        </td>

                                        <td>
                                            {formatDate(req.raisedOn)}
                                        </td>

                                        <td>
                                            <span
                                                className={getStatusClass(
                                                    req.reqStatus
                                                )}
                                            >
                                                {getStatusName(req.reqStatus)}
                                            </span>
                                        </td>

                                        <td>
                                            <div className="action-buttons">

                                                <button
                                                    type="button"
                                                    className="btn-view-details"
                                                    onClick={(e) => {
                                                        e.stopPropagation();
                                                        setSelectedRequest(req);
                                                    }}
                                                >
                                                    View Details
                                                </button>

                                                {(req.reqStatus === 1 ||
                                                    req.reqStatus === 3) && (
                                                        <button
                                                            type="button"
                                                            onClick={(e) => {
                                                                e.stopPropagation();
                                                                closeRequest(req.requestId);
                                                            }}
                                                            className="btn btn-primary"
                                                        >
                                                        Mark as Resolved
                                                        </button>
                                                    )}

                                                <Link
                                                    to={`/delete/${req.requestId}`}
                                                    className="btn-delete"
                                                    onClick={(e) =>
                                                        e.stopPropagation()
                                                    }
                                                >
                                                    Delete
                                                </Link>

                                            </div>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    )}

                    {/* PAGINATION */}
                    <div className="pagination">
                        <p>
                            Showing {requests.length} records on page {pageNumber}
                            {" "}of {totalPages} | Total records: {totalRecords}
                        </p>

                        <div className="pagination-controls">
                            <button
                                disabled={pageNumber <= 1 || loading}
                                onClick={() =>
                                    setPageNumber(page => page - 1)
                                }
                            >
                                Previous
                            </button>

                            <span>
                                Page {pageNumber} of {totalPages || 1}
                            </span>

                            <button
                                disabled={
                                    pageNumber >= totalPages || loading
                                }
                                onClick={() =>
                                    setPageNumber(page => page + 1)
                                }
                            >
                                Next
                            </button>

                            <select
                                value={pageSize}
                                onChange={(e) => {
                                    setPageSize(Number(e.target.value));
                                    setPageNumber(1);
                                }}
                            >
                                <option value={5}>5 per page</option>
                                <option value={10}>10 per page</option>
                                <option value={20}>20 per page</option>
                                <option value={50}>50 per page</option>
                            </select>
                        </div>
                    </div>
                </div>
            </div>

            {/* REQUEST DETAILS MODAL */}
            {selectedRequest && (
                <div
                    className="request-modal-overlay"
                    onClick={() => setSelectedRequest(null)}
                >
                    <div
                        className="request-modal"
                        onClick={(e) => e.stopPropagation()}
                        role="dialog"
                        aria-modal="true"
                        aria-labelledby="request-modal-title"
                    >
                        {/* MODAL HEADER */}
                        <div className="request-modal-header">
                            <div>
                                <span className="request-modal-eyebrow">
                                    SERVICE REQUEST DETAILS
                                </span>

                                <h2 id="request-modal-title">
                                    Request #{selectedRequest.requestId}
                                </h2>

                                <p>
                                    Review the complete information submitted
                                    by the requester.
                                </p>
                            </div>

                            <button
                                type="button"
                                className="request-modal-close"
                                onClick={() => setSelectedRequest(null)}
                                aria-label="Close details"
                            >
                                ×
                            </button>
                        </div>

                        {/* MODAL BODY */}
                        <div className="request-modal-body">

                            {/* STATUS */}
                            <div className="request-detail-status">
                                <div>
                                    <span className="detail-label">
                                        Current Request Status
                                    </span>

                                    <span
                                        className={getStatusClass(
                                            selectedRequest.reqStatus
                                        )}
                                    >
                                        {getStatusName(
                                            selectedRequest.reqStatus
                                        )}
                                    </span>
                                </div>

                                <div className="request-detail-id">
                                    <span className="detail-label">
                                        Request Reference
                                    </span>

                                    <strong>
                                        #{selectedRequest.requestId}
                                    </strong>
                                </div>
                            </div>

                            {/* REQUEST INFORMATION */}
                            <h3 className="request-section-heading">
                                Request Information
                            </h3>

                            <div className="request-detail-grid">

                                <div className="request-detail-card">
                                    <span className="detail-label">
                                        Raised By
                                    </span>

                                    <strong>
                                        {selectedRequest.raisedBy ||
                                            "Not available"}
                                    </strong>
                                </div>

                                <div className="request-detail-card">
                                    <span className="detail-label">
                                        Created On
                                    </span>

                                    <strong>
                                        {formatDate(selectedRequest.raisedOn)}
                                    </strong>
                                </div>

                            </div>

                            {/* DESCRIPTION */}
                            <div className="request-detail-section">
                                <h3>
                                    <span>01</span> Request Description
                                </h3>

                                <p>
                                    {selectedRequest.description ||
                                        "No description provided."}
                                </p>
                            </div>

                            {/* DETAILS */}
                            <div className="request-detail-section">
                                <h3>
                                    <span>02</span> Detailed Explanation
                                </h3>

                                <p className="request-detail-text">
                                    {selectedRequest.details ||
                                        "No additional details provided."}
                                </p>
                            </div>

                            {/* JUSTIFICATION */}
                            <div className="request-detail-section">
                                <h3>
                                    <span>03</span>{" "}
                                    {selectedRequest.justification &&
                                        selectedRequest.justification !== "Initial Request"
                                        ? "Reopening Justification"
                                        : "Initial Request Justification"}
                                </h3>

                                <p className="request-detail-text">
                                    {selectedRequest.justification ||
                                        "No justification provided."}
                                </p>
                            </div>

                        </div>

                        {/* MODAL FOOTER */}
                        <div className="request-modal-footer">
                            <span>
                                IT Service Portal · Request #
                                {selectedRequest.requestId}
                            </span>

                            <button
                                type="button"
                                className="btn-secondary"
                                onClick={() => setSelectedRequest(null)}
                            >
                                Close Details
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </>
    );
}

export default AdminDashboard;


import { useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import API from "../services/api";

function AuditHistory() {
    const [auditLogs, setAuditLogs] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");

    // Filters
    const [userName, setUserName] = useState("");
    const [action, setAction] = useState("");
    const [fromDate, setFromDate] = useState("");
    const [toDate, setToDate] = useState("");

    // Pagination
    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [totalRecords, setTotalRecords] = useState(0);
    const [totalPages, setTotalPages] = useState(0);

    // Request-specific activity
    const [selectedRequestId, setSelectedRequestId] = useState(null);

    // Audit summary
    const [summary, setSummary] = useState({
        totalActivities: 0,
        createdCount: 0,
        closedCount: 0,
        reopenedCount: 0,
        deletedCount: 0
    });

    // Fetch audit records
    const fetchAuditLogs = useCallback(async (
        page = 1,
        requestId = null,
        filters = {}
    ) => {
        try {
            setLoading(true);
            setError("");

            const response = await API.get(
                "/ITSRPAPI/AuditHistory",
                {
                    params: {
                        pageNumber: page,
                        pageSize,
                        requestId: requestId || undefined,
                        userName: filters.userName?.trim() || undefined,
                        action: filters.action || undefined,
                        fromDate: filters.fromDate || undefined,
                        toDate: filters.toDate || undefined
                    }
                }
            );

            setAuditLogs(response.data.items || []);
            setTotalRecords(response.data.totalRecords || 0);
            setTotalPages(response.data.totalPages || 0);
            setPageNumber(page);

        } catch (err) {
            console.error("Audit history error:", err);

            if (err.response?.status === 403) {
                setError("You are not authorized to view audit history.");
            } else if (err.response?.status === 404) {
                setError("Audit history endpoint was not found.");
            } else {
                setError("Unable to load audit history.");
            }
        } finally {
            setLoading(false);
        }
    }, [pageSize]);

    // Fetch audit summary
    const fetchAuditSummary = useCallback(async () => {
        try {
            const response = await API.get(
                "/ITSRPAPI/AuditSummary"
            );

            setSummary(response.data);
        } catch (err) {
            console.error("Unable to load audit summary:", err);
        }
    }, []);

    // Load initial data and reload records when page size changes
    useEffect(() => {
        fetchAuditLogs(1);
        fetchAuditSummary();
    }, [pageSize, fetchAuditLogs, fetchAuditSummary]);

    // Current filter values
    const getCurrentFilters = () => ({
        userName,
        action,
        fromDate,
        toDate
    });

    // Apply filters
    const handleFilter = () => {
        if (fromDate && toDate && fromDate > toDate) {
            setError("From date cannot be after To date.");
            return;
        }

        setSelectedRequestId(null);
        fetchAuditLogs(1, null, getCurrentFilters());
    };

    // Clear filters
    const clearFilters = () => {
        setUserName("");
        setAction("");
        setFromDate("");
        setToDate("");
        setSelectedRequestId(null);

        fetchAuditLogs(1, null, {
            userName: "",
            action: "",
            fromDate: "",
            toDate: ""
        });
    };

    // View activities for one request
    const handleRequestClick = (requestId) => {
        setSelectedRequestId(requestId);

        fetchAuditLogs(
            1,
            requestId,
            getCurrentFilters()
        );
    };

    // Show all activities
    const handleShowAll = () => {
        setSelectedRequestId(null);

        fetchAuditLogs(
            1,
            null,
            getCurrentFilters()
        );
    };

    // Format date
    const formatDate = (date) => {
        if (!date) return "-";

        return new Date(date).toLocaleString();
    };

    // Action badge styling
    const getActionClass = (actionName) => {
        switch (actionName?.toLowerCase()) {
            case "created":
                return "audit-created";
            case "closed":
                return "audit-closed";
            case "reopened":
                return "audit-reopened";
            case "deleted":
                return "audit-deleted";
            default:
                return "audit-default";
        }
    };

    return (
        <div className="audit-page">

            {/* Header */}
            <div className="audit-header">
                <div>
                    <h1>Audit History</h1>
                    <p>Monitor service request activities.</p>
                </div>

                <Link
                    to="/admin/dashboard"
                    className="audit-back"
                >
                    Back to Dashboard
                </Link>
            </div>

            {/* Summary Cards */}
            <div className="audit-summary">

                <div className="audit-summary-card">
                    <span>Total Activities</span>
                    <h2>{summary.totalActivities}</h2>
                </div>

                <div className="audit-summary-card">
                    <span>Created</span>
                    <h2>{summary.createdCount}</h2>
                </div>

                <div className="audit-summary-card">
                    <span>Closed</span>
                    <h2>{summary.closedCount}</h2>
                </div>

                <div className="audit-summary-card">
                    <span>Reopened</span>
                    <h2>{summary.reopenedCount}</h2>
                </div>

                <div className="audit-summary-card">
                    <span>Deleted</span>
                    <h2>{summary.deletedCount}</h2>
                </div>

            </div>

            {/* Filters */}
            <div className="audit-filters">

                <h3>Filter Activities</h3>

                <div className="audit-filter-grid">

                    <div>
                        <label>Username</label>
                        <input
                            type="text"
                            value={userName}
                            onChange={(e) => setUserName(e.target.value)}
                            placeholder="Enter username"
                        />
                    </div>

                    <div>
                        <label>Action</label>
                        <select
                            value={action}
                            onChange={(e) => setAction(e.target.value)}
                        >
                            <option value="">All Actions</option>
                            <option value="Created">Created</option>
                            <option value="Closed">Closed</option>
                            <option value="Reopened">Reopened</option>
                            <option value="Deleted">Deleted</option>
                        </select>
                    </div>

                    <div>
                        <label>From Date</label>
                        <input
                            type="date"
                            value={fromDate}
                            onChange={(e) => setFromDate(e.target.value)}
                        />
                    </div>

                    <div>
                        <label>To Date</label>
                        <input
                            type="date"
                            value={toDate}
                            onChange={(e) => setToDate(e.target.value)}
                        />
                    </div>

                </div>

                <div className="audit-filter-actions">

                    <button
                        onClick={handleFilter}
                        disabled={loading}
                    >
                        Apply Filters
                    </button>

                    <button
                        onClick={clearFilters}
                        className="audit-clear"
                        disabled={loading}
                    >
                        Clear Filters
                    </button>

                </div>

            </div>

            {/* Activity Records */}
            <div className="audit-table-container">

                <div className="audit-table-heading">
                    <h3>Activity Records</h3>

                    <select
                        value={pageSize}
                        onChange={(e) => setPageSize(Number(e.target.value))}
                    >
                        <option value={5}>5 per page</option>
                        <option value={10}>10 per page</option>
                        <option value={20}>20 per page</option>
                        <option value={50}>50 per page</option>
                    </select>
                </div>

                {/* Selected Request Filter */}
                {selectedRequestId !== null && (
                    <div className="selected-request-filter">
                        <span>
                            Showing activity for Request #{selectedRequestId}
                        </span>

                        <button
                            type="button"
                            onClick={handleShowAll}
                            disabled={loading}
                        >
                            Show All Activities
                        </button>
                    </div>
                )}

                {/* Loading and Error Messages */}
                {loading ? (
                    <p className="audit-message">
                        Loading audit records...
                    </p>
                ) : error ? (
                    <p className="audit-error">{error}</p>
                ) : auditLogs.length === 0 ? (
                    <p className="audit-message">
                        No audit records found.
                    </p>
                ) : (

                    <div className="audit-table-scroll">

                        <table className="audit-table">

                            <thead>
                                <tr>
                                    <th>S.No.</th>
                                    <th>Request ID</th>
                                    <th>Action</th>
                                    <th>Performed By</th>
                                    <th>Date &amp; Time</th>
                                    <th>Details</th>
                                </tr>
                            </thead>

                            <tbody>

                                {auditLogs.map((log, index) => (

                                    <tr key={log.auditId}>

                                        {/* Continuous serial number across pages */}
                                        <td>
                                            {(pageNumber - 1) * pageSize + index + 1}
                                        </td>

                                        

                                        <td>
                                            <button
                                                type="button"
                                                className="request-id-link"
                                                onClick={() =>
                                                    handleRequestClick(log.requestId)
                                                }
                                            >
                                                {log.requestId}
                                            </button>
                                        </td>

                                        <td>
                                            <span
                                                className={getActionClass(log.action)}
                                            >
                                                {log.action}
                                            </span>
                                        </td>

                                        <td>{log.performedBy}</td>

                                        <td>{formatDate(log.performedAt)}</td>

                                        <td>{log.details || "-"}</td>

                                    </tr>

                                ))}

                            </tbody>

                        </table>

                    </div>
                )}

                {/* Pagination */}
                <div className="audit-pagination">

                    <span>
                        Showing {auditLogs.length} of {totalRecords} records
                    </span>

                    <div>

                        <button
                            disabled={pageNumber <= 1 || loading}
                            onClick={() =>
                                fetchAuditLogs(
                                    pageNumber - 1,
                                    selectedRequestId,
                                    getCurrentFilters()
                                )
                            }
                        >
                            Previous
                        </button>

                        <span>
                            Page {pageNumber} of {totalPages}
                        </span>

                        <button
                            disabled={pageNumber >= totalPages || loading}
                            onClick={() =>
                                fetchAuditLogs(
                                    pageNumber + 1,
                                    selectedRequestId,
                                    getCurrentFilters()
                                )
                            }
                        >
                            Next
                        </button>

                    </div>

                </div>

            </div>

        </div>
    );
}

export default AuditHistory;

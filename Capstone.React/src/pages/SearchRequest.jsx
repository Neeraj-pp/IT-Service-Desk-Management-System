
import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import API from "../services/api";

function SearchRequest() {
    const navigate = useNavigate();

    const [requests, setRequests] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    // Filter input values
    const [filters, setFilters] = useState({
        searchTerm: "",
        userName: "",
        statusId: "",
        fromDate: "",
        toDate: ""
    });

    // Filters currently applied to the API
    const [appliedFilters, setAppliedFilters] = useState({
        searchTerm: "",
        userName: "",
        statusId: "",
        fromDate: "",
        toDate: ""
    });

    // Pagination
    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [totalRecords, setTotalRecords] = useState(0);
    const [totalPages, setTotalPages] = useState(0);

    useEffect(() => {
        let user = null;

        try {
            user = JSON.parse(localStorage.getItem("user"));
        } catch {
            user = null;
        }

        if (!user || user.role?.roleName !== "Admin") {
            navigate("/", { replace: true });
            return;
        }

        const loadRequests = async () => {
            try {
                setLoading(true);
                setError("");

                const response = await API.get(
                    "/ITSRPAPI/GetAllRequest",
                    {
                        params: {
                            pageNumber,
                            pageSize,
                            searchTerm: appliedFilters.searchTerm || undefined,
                            userName: appliedFilters.userName || undefined,
                            statusId: appliedFilters.statusId || undefined,
                            fromDate: appliedFilters.fromDate || undefined,
                            toDate: appliedFilters.toDate || undefined
                        }
                    }
                );

                const data = response.data;

                setRequests(data.items || []);
                setTotalRecords(data.totalRecords || 0);
                setTotalPages(data.totalPages || 0);

            } catch (error) {
                console.error("Error loading requests:", error);

                setRequests([]);
                setTotalRecords(0);
                setTotalPages(0);

                if (error.response?.status === 401) {
                    setError("Your session has expired. Please login again.");
                } else if (error.response?.status === 403) {
                    setError("You are not authorized to view all requests.");
                } else {
                    setError("Unable to load requests. Please try again.");
                }
            } finally {
                setLoading(false);
            }
        };

        // The request is asynchronous; loading state is updated inside it.
        // eslint-disable-next-line react-hooks/set-state-in-effect
        loadRequests();

    }, [navigate, pageNumber, pageSize, appliedFilters]);

    const handleFilterChange = (e) => {
        const { name, value } = e.target;

        setFilters((previous) => ({
            ...previous,
            [name]: value
        }));
    };

    const searchRequests = () => {
        if (
            filters.fromDate &&
            filters.toDate &&
            filters.fromDate > filters.toDate
        ) {
            setError("From date cannot be after To date.");
            return;
        }

        setError("");
        setPageNumber(1);

        setAppliedFilters({
            searchTerm: filters.searchTerm.trim(),
            userName: filters.userName.trim(),
            statusId: filters.statusId,
            fromDate: filters.fromDate,
            toDate: filters.toDate
        });
    };

    const clearSearch = () => {
        const emptyFilters = {
            searchTerm: "",
            userName: "",
            statusId: "",
            fromDate: "",
            toDate: ""
        };

        setFilters(emptyFilters);
        setAppliedFilters(emptyFilters);
        setPageNumber(1);
        setError("");
    };

    const getStatusName = (statusId) => {
        switch (statusId) {
            case 1:
                return "New";
            case 2:
                return "Closed";
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
                return "status-closed";
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
            </div>

            <div className="content-wrapper">
                <h1 className="page-title">Search Requests</h1>

                <div className="top-bar">
                    <Link
                        to="/admin/dashboard"
                        className="btn btn-danger"
                    >
                        Back
                    </Link>
                </div>

                {/* Search and filter section */}
                <div className="search-box">

                    <div className="form-group">
                        <label>Search Description / Details</label>
                        <input
                            type="text"
                            name="searchTerm"
                            placeholder="Enter keywords"
                            value={filters.searchTerm}
                            onChange={handleFilterChange}
                            className="search-input"
                        />
                    </div>

                    <div className="form-group">
                        <label>Username</label>
                        <input
                            type="text"
                            name="userName"
                            placeholder="Enter username"
                            value={filters.userName}
                            onChange={handleFilterChange}
                            className="search-input"
                        />
                    </div>

                    <div className="form-group">
                        <label>Status</label>
                        <select
                            name="statusId"
                            value={filters.statusId}
                            onChange={handleFilterChange}
                            className="search-input"
                        >
                            <option value="">All Statuses</option>
                            <option value="1">New</option>
                            <option value="2">Closed</option>
                            <option value="3">Reopened</option>
                        </select>
                    </div>

                    <div className="form-group">
                        <label>From Date</label>
                        <input
                            type="date"
                            name="fromDate"
                            value={filters.fromDate}
                            onChange={handleFilterChange}
                            className="search-input"
                        />
                    </div>

                    <div className="form-group">
                        <label>To Date</label>
                        <input
                            type="date"
                            name="toDate"
                            value={filters.toDate}
                            onChange={handleFilterChange}
                            className="search-input"
                        />
                    </div>

                    <div className="form-actions">
                        <button
                            onClick={searchRequests}
                            className="btn btn-primary"
                            disabled={loading}
                        >
                            Apply Filters
                        </button>

                        <button
                            onClick={clearSearch}
                            className="btn btn-secondary"
                            disabled={loading}
                        >
                            Clear Filters
                        </button>
                    </div>
                </div>

                {error && (
                    <p className="error">{error}</p>
                )}

                {/* Results */}
                {loading ? (
                    <h3>Loading requests...</h3>
                ) : (
                    <>
                        <p>
                            Total requests found: <strong>{totalRecords}</strong>
                        </p>

                        <div className="table-container">
                            <table border="1" cellPadding="10">
                                <thead>
                                    <tr>
                                        <th>Request ID</th>
                                        <th>Description</th>
                                        <th>Details</th>
                                        <th>Requested By</th>
                                        <th>Creation Date</th>
                                        <th>Status</th>
                                    </tr>
                                </thead>

                                <tbody>
                                    {requests.length > 0 ? (
                                        requests.map((req) => (
                                            <tr key={req.requestId}>
                                                <td>{req.requestId}</td>
                                                <td>{req.description}</td>
                                                <td>{req.details}</td>
                                                <td>{req.raisedBy}</td>
                                                <td>
                                                    {new Date(
                                                        req.raisedOn
                                                    ).toLocaleString()}
                                                </td>
                                                <td>
                                                    <span
                                                        className={getStatusClass(
                                                            req.reqStatus
                                                        )}
                                                    >
                                                        {getStatusName(
                                                            req.reqStatus
                                                        )}
                                                    </span>
                                                </td>
                                            </tr>
                                        ))
                                    ) : (
                                        <tr>
                                            <td colSpan="6">
                                                No requests found.
                                            </td>
                                        </tr>
                                    )}
                                </tbody>
                            </table>
                        </div>

                        {/* Pagination */}
                        <div className="form-actions">
                            <button
                                className="btn btn-secondary"
                                disabled={pageNumber <= 1 || loading}
                                onClick={() =>
                                    setPageNumber((previous) => previous - 1)
                                }
                            >
                                Previous
                            </button>

                            <span>
                                Page {pageNumber} of {totalPages || 1}
                            </span>

                            <button
                                className="btn btn-primary"
                                disabled={
                                    pageNumber >= totalPages || loading
                                }
                                onClick={() =>
                                    setPageNumber((previous) => previous + 1)
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
                                className="search-input"
                                disabled={loading}
                            >
                                <option value={5}>5 per page</option>
                                <option value={10}>10 per page</option>
                                <option value={20}>20 per page</option>
                                <option value={50}>50 per page</option>
                            </select>
                        </div>
                    </>
                )}
            </div>
        </>
    );
}

export default SearchRequest;

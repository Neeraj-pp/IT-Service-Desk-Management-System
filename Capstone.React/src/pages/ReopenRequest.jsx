
import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import API from "../services/api";

function ReopenRequest() {
    const { id } = useParams();
    const navigate = useNavigate();

    const [request, setRequest] = useState(null);
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {
        const loadRequest = async () => {
            let user = null;

            try {
                user = JSON.parse(localStorage.getItem("user"));
            } catch {
                user = null;
            }

            if (!user || user.role?.roleName !== "User") {
                navigate("/", { replace: true });
                return;
            }

            try {
                setLoading(true);
                setError("");

                const response = await API.get(
                    `/ITSRPAPI/GetRequestById?reqId=${id}`
                );

                setRequest(response.data);

                if (response.data.reqStatus !== 2) {
                    setError("Only resolved requests can be reopened.");
                }
            } catch (error) {
                console.error("Error loading request:", error);

                if (error.response?.status === 404) {
                    setError("Request not found.");
                } else if (error.response?.status === 403) {
                    setError("You are not allowed to access this request.");
                } else {
                    setError("Unable to load request. Please try again.");
                }
            } finally {
                setLoading(false);
            }
        };

        loadRequest();
    }, [navigate, id]);

    const reopenRequest = async () => {
        if (!request?.justification?.trim()) {
            setError("Justification is required.");
            return;
        }

        if (request.reqStatus !== 2) {
            setError("Only resolved requests can be reopened.");
            return;
        }

        try {
            setSubmitting(true);
            setError("");

            await API.put("/ITSRPAPI/reopen", {
                requestId: request.requestId,
                details: request.details,
                justification: request.justification.trim()
            });

            alert("Request reopened successfully.");
            navigate("/user/dashboard", { replace: true });

        } catch (error) {
            console.error("Reopen error:", error);

            if (error.response?.status === 404) {
                setError("Request not found.");
            } else if (error.response?.status === 403) {
                setError("You are not allowed to reopen this request.");
            } else if (error.response?.status === 400) {
                const responseData = error.response.data;

                setError(
                    typeof responseData === "string"
                        ? responseData
                        : responseData?.errors?.Justification?.[0] ||
                        "Please enter a valid justification."
                );
            } else {
                setError("Unable to reopen request. Please try again.");
            }
        } finally {
            setSubmitting(false);
        }
    };

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
                <h2>Reopen Request</h2>
                <p className="error">
                    {error || "Request not found."}
                </p>

                <button
                    onClick={() => navigate("/user/dashboard")}
                    className="btn btn-secondary"
                >
                    Back
                </button>
            </div>
        );
    }

    const canReopen = request.reqStatus === 2;

    return (
        <div className="request-page">
            <div className="request-card">
                <h2>Reopen Request</h2>

                <p className="request-subtitle">
                    Provide a justification explaining why this resolved request needs further attention.
                </p>

                <div className="form-group">
                    <label>Description</label>
                    <input
                        className="input-field"
                        type="text"
                        value={request.description || ""}
                        readOnly
                    />
                </div>

                <div className="form-group">
                    <label>Details</label>
                    <textarea
                        className="textarea-field"
                        value={request.details || ""}
                        readOnly
                    />
                </div>

                <div className="form-group">
                    <label>Creation Date</label>
                    <input
                        className="input-field"
                        type="text"
                        value={
                            request.raisedOn
                                ? new Date(request.raisedOn).toLocaleString()
                                : ""
                        }
                        readOnly
                    />
                </div>

                <div className="form-group">
                    <label>
                        Justification <span className="required">*</span>
                    </label>

                    <textarea
                        className="textarea-field"
                        placeholder="Enter the reason for reopening this request"
                        value={request.justification || ""}
                        disabled={!canReopen || submitting}
                        onChange={(e) => {
                            setRequest({
                                ...request,
                                justification: e.target.value
                            });

                            if (e.target.value.trim()) {
                                setError("");
                            }
                        }}
                    />
                </div>

                {error && (
                    <p className="request-error">
                        {error}
                    </p>
                )}

                <div className="form-actions">
                    <button
                        onClick={() => navigate("/user/dashboard")}
                        className="btn btn-secondary"
                        disabled={submitting}
                    >
                        Back
                    </button>

                    <button
                        onClick={reopenRequest}
                        className="btn btn-primary"
                        disabled={!canReopen || submitting}
                    >
                        {submitting ? "Reopening..." : "Reopen Request"}
                    </button>
                </div>
            </div>
        </div>
    );
}

export default ReopenRequest;

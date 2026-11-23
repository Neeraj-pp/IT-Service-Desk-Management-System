
import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import API from "../services/api";

function AddRequest() {
    const [description, setDescription] = useState("");
    const [details, setDetails] = useState("");

    const [fieldErrors, setFieldErrors] = useState({
        description: "",
        details: ""
    });

    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");
    const [loading, setLoading] = useState(false);

    const navigate = useNavigate();

    useEffect(() => {
        let user = null;

        try {
            user = JSON.parse(localStorage.getItem("user"));
        } catch {
            user = null;
        }

        if (!user || user.role?.roleName !== "User") {
            navigate("/", { replace: true });
        }
    }, [navigate]);

    const validateForm = () => {
        const errors = {
            description: "",
            details: ""
        };

        if (!description.trim()) {
            errors.description = "Description is required.";
        } else if (description.trim().length > 50) {
            errors.description = "Description cannot exceed 50 characters.";
        }

        if (!details.trim()) {
            errors.details = "Details are required.";
        } else if (details.trim().length > 100) {
            errors.details = "Details cannot exceed 100 characters.";
        }

        setFieldErrors(errors);

        return !errors.description && !errors.details;
    };

    const saveRequest = async () => {
        setError("");
        setFieldErrors({
            description: "",
            details: ""
        });

        if (!validateForm()) {
            return;
        }

        try {
            setLoading(true);

            const request = {
                description: description.trim(),
                details: details.trim(),
                justification: "Initial Request"
            };

            const response = await API.post(
                "/ITSRPAPI/CreateNewSerRequest",
                request
            );

            const requestId = response.data?.requestId;

            setSuccess(
                requestId
                    ? `Your service request has been submitted successfully.`
                    : "Your service request has been submitted successfully."
            );

            setDescription("");
            setDetails("");

        } catch (err) {
            console.error("Error creating request:", err);

            if (err.response?.status === 400) {
                const serverErrors = err.response.data?.errors;

                if (serverErrors) {
                    const descriptionError =
                        serverErrors.Description ||
                        serverErrors.description;

                    const detailsError =
                        serverErrors.Details ||
                        serverErrors.details;

                    setFieldErrors({
                        description: Array.isArray(descriptionError)
                            ? descriptionError[0]
                            : descriptionError || "",
                        details: Array.isArray(detailsError)
                            ? detailsError[0]
                            : detailsError || ""
                    });

                    if (!descriptionError && !detailsError) {
                        setError(
                            err.response.data?.title ||
                            "Please check the information you entered."
                        );
                    }
                } else {
                    setError(
                        err.response.data?.title ||
                        "Please check the information you entered."
                    );
                }
            } else if (err.response?.status === 401) {
                setError("Your session has expired. Please login again.");
            } else {
                setError("Unable to create request. Please try again.");
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="request-page">
            <div className="request-card">

                {success ? (
                    <div className="request-success-container">
                        <div className="request-success-icon">
                            ✓
                        </div>

                        <h1>Request Submitted!</h1>

                        <p className="request-success">
                            {success}
                        </p>

                        <p className="request-subtitle">
                            You can track the progress of your request
                            from your dashboard.
                        </p>

                        <button
                            type="button"
                            className="btn btn-primary"
                            onClick={() =>
                                navigate("/user/dashboard", {
                                    replace: true
                                })
                            }
                        >
                            View My Requests
                        </button>
                    </div>
                ) : (
                    <>
                        <h1>Raise New Request</h1>

                        <p className="request-subtitle">
                            Submit a new IT service request to the support team.
                        </p>

                        <div className="form-group">
                            <label>
                                Description <span className="required">*</span>
                            </label>

                            <input
                                className={`input-field ${fieldErrors.description ? "input-invalid" : ""
                                    }`}
                                type="text"
                                maxLength={50}
                                placeholder="Enter a short description"
                                value={description}
                                disabled={loading}
                                aria-invalid={!!fieldErrors.description}
                                aria-describedby="description-error"
                                onChange={(e) => {
                                    setDescription(e.target.value);
                                    setFieldErrors(prev => ({
                                        ...prev,
                                        description: ""
                                    }));
                                    setError("");
                                }}
                            />

                            <small>{description.length}/50 characters</small>

                            {fieldErrors.description && (
                                <p
                                    id="description-error"
                                    className="field-error"
                                >
                                    {fieldErrors.description}
                                </p>
                            )}
                        </div>

                        <div className="form-group">
                            <label>
                                Details <span className="required">*</span>
                            </label>

                            <textarea
                                className={`textarea-field ${fieldErrors.details ? "input-invalid" : ""
                                    }`}
                                maxLength={100}
                                placeholder="Describe your issue in detail"
                                value={details}
                                disabled={loading}
                                aria-invalid={!!fieldErrors.details}
                                aria-describedby="details-error"
                                onChange={(e) => {
                                    setDetails(e.target.value);
                                    setFieldErrors(prev => ({
                                        ...prev,
                                        details: ""
                                    }));
                                    setError("");
                                }}
                            />

                            <small>{details.length}/100 characters</small>

                            {fieldErrors.details && (
                                <p
                                    id="details-error"
                                    className="field-error"
                                >
                                    {fieldErrors.details}
                                </p>
                            )}
                        </div>

                        {error && (
                            <p className="request-error">
                                {error}
                            </p>
                        )}

                        <div className="form-actions">
                            <button
                                type="button"
                                onClick={() => navigate("/user/dashboard")}
                                className="btn btn-secondary"
                                disabled={loading}
                            >
                                Back
                            </button>

                            <button
                                type="button"
                                onClick={saveRequest}
                                className="btn btn-primary"
                                disabled={loading}
                            >
                                {loading ? "Submitting..." : "Save Request"}
                            </button>
                        </div>
                    </>
                )}

            </div>
        </div>
    );
}

export default AddRequest;

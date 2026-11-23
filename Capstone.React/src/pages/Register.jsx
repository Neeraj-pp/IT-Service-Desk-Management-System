import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import API from "../services/api";

function Register() {
    const [userName, setUserName] = useState("");
    const [password, setPassword] = useState("");
    const [showPasswords, setShowPasswords] = useState(false);
    const [confirmPassword, setConfirmPassword] = useState("");
    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();

    const handleRegister = async (event) => {
        event.preventDefault();
        setError("");
        setSuccess("");

        const trimmedUserName = userName.trim();

        if (!trimmedUserName || !password || !confirmPassword) {
            setError("Please complete all fields.");
            return;
        }

        if (trimmedUserName.length > 20) {
            setError("Username cannot be longer than 20 characters.");
            return;
        }

        if (password !== confirmPassword) {
            setError("Passwords do not match.");
            return;
        }

        try {
            await API.post("/ITSRPAPI/Register", {
    userName: trimmedUserName,
    password,
    confirmPassword
});

            setSuccess("Your account has been created. Redirecting you to sign in...");
            setTimeout(() => navigate("/", {
                replace: true,
                state: { registrationSuccess: true, userName: trimmedUserName }
            }), 900);
        } catch (requestError) {
            console.error("Registration error:", requestError);
            const status = requestError.response?.status;
            const data = requestError.response?.data;

            if (status === 409) {
                setError("That username is already registered. Please choose another.");
            } else if (status === 400) {
                const validationMessage =
                    data?.errors && Object.values(data.errors).flat().find(Boolean);
                setError(
                    validationMessage ||
                    (typeof data === "string" ? data : data?.message) ||
                    "Please check your details and try again."
                );
            } else {
                setError("Unable to create your account right now. Please try again.");
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <main className="auth-page">
            <section className="auth-card auth-card-register" aria-labelledby="register-title">
                <div className="auth-brand">
                    <div className="auth-brand-mark" aria-hidden="true">IT</div>
                    <span>IT Service Portal</span>
                </div>

                <div className="auth-heading">
                    <p className="auth-eyebrow">GET STARTED</p>
                    <h1 id="register-title">Create your account</h1>
                    <p>Register to raise and track your service requests.</p>
                </div>

                <form onSubmit={handleRegister} noValidate>
                    <div className="auth-field">
                        <label htmlFor="register-username">Username</label>
                        <input
                            id="register-username"
                            type="text"
                            autoComplete="username"
                            maxLength={20}
                            placeholder="Choose a username"
                            value={userName}
                            onChange={(event) => {
                                setUserName(event.target.value);
                                setError("");
                            }}
                            disabled={loading}
                        />
                        <small>Maximum 20 characters.</small>
                    </div>

                    <div className="auth-field">
                        <label htmlFor="register-password">Password</label>
                        <input
                            id="register-password"
                            type={showPasswords ? "text" : "password"}
                            autoComplete="new-password"
                            placeholder="Create a password"
                            value={password}
                            onChange={(event) => {
                                setPassword(event.target.value);
                                setError("");
                            }}
                            disabled={loading}
                        />
                    </div>
                    

                    <div className="auth-field">
                        <label htmlFor="register-confirm-password">Confirm password</label>
                        <input
                            id="register-confirm-password"
                            type={showPasswords ? "text" : "password"}
                            autoComplete="new-password"
                            placeholder="Re-enter your password"
                            value={confirmPassword}
                            onChange={(event) => {
                                setConfirmPassword(event.target.value);
                                setError("");
                            }}
                            disabled={loading}
                        />
                    </div>
                    <label className="auth-show-password">
                        <input
                            type="checkbox"
                            checked={showPasswords}
                            onChange={(e) => setShowPasswords(e.target.checked)}
                        />
                        <span>Show passwords</span>
                    </label>

                    <p className="auth-helper">
                        New accounts are registered with the standard User role.
                    </p>

                    {error && <p className="auth-alert" role="alert">{error}</p>}
                    {success && <p className="auth-success" role="status">{success}</p>}

                    <button className="auth-submit" type="submit" disabled={loading}>
                        {loading ? "Creating account..." : "Create account"}
                    </button>
                </form>

                <p className="auth-switch">
                    Already have an account? <Link to="/">Sign in</Link>
                </p>
                <p className="auth-footnote">Your password is securely hashed by the server.</p>
            </section>
        </main>
    );
}

export default Register;

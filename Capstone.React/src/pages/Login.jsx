import { useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import API from "../services/api";

function Login() {
    const [userName, setUserName] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();
    const location = useLocation();
    const [showPasswords, setShowPasswords] = useState(false);


    const handleLogin = async (event) => {
        event.preventDefault();
        setError("");

        const trimmedUserName = userName.trim();

        if (!trimmedUserName || !password) {
            setError("Username and password are required.");
            return;
        }

        try {
            setLoading(true);

            const response = await API.post("/ITSRPAPI/Login", {
                userName: trimmedUserName,
                password
            });

            const data = response.data;

            // Validate the complete login response before saving the session.
            const validRoles = ["Admin", "User"];
            const expiryTime = Date.parse(data?.expiresAtUtc);

            if (
                !data?.accessToken ||
                !data?.user?.userName ||
                !validRoles.includes(data?.user?.role) ||
                !Number.isFinite(expiryTime) ||
                expiryTime <= Date.now()
            ) {
                setError("The server returned an invalid login response. Please try again.");
                return;
            }

            const user = {
                userName: data.user.userName,
                role: { roleName: data.user.role }
            };

            // Store the session only after successful validation.
            localStorage.setItem("accessToken", data.accessToken);
            localStorage.setItem("user", JSON.stringify(user));
            localStorage.setItem("tokenExpiresAt", data.expiresAtUtc);

            if (data.user.role === "Admin") {
                navigate("/admin/dashboard", { replace: true });
            } else {
                navigate("/user/dashboard", { replace: true });
            }

        } catch (error) {
            if (error.response?.status === 401) {
                setError("Invalid username or password.");
            } else if (error.response?.status === 400) {
                setError("Please enter valid login details.");
            } else {
                setError("Unable to connect to the server. Please try again.");
            }
        } finally {
            setLoading(false);
        }
    };


    return (
        <main className="auth-page">
            <section className="auth-card" aria-labelledby="login-title">
                <div className="auth-brand">
                    <div className="auth-brand-mark" aria-hidden="true">IT</div>
                    <span>IT Service Portal</span>
                </div>

                <div className="auth-heading">
                    <p className="auth-eyebrow">WELCOME BACK</p>
                    <h1 id="login-title">Sign in to your account</h1>
                    <p>Access and manage your service requests.</p>
                </div>

                {location.state?.registrationSuccess && (
                    <p className="auth-success" role="status">
                        Account created successfully. You can now sign in.
                    </p>
                )}

                <form onSubmit={handleLogin} noValidate>
                    <div className="auth-field">
                        <label htmlFor="login-username">Username</label>
                        <input
                            id="login-username"
                            type="text"
                            autoComplete="username"
                            maxLength={20}
                            placeholder="Enter your username"
                            value={userName}
                            onChange={(event) => {
                                setUserName(event.target.value);
                                setError("");
                            }}
                            disabled={loading}
                        />
                    </div>

                    <div className="auth-field">
                        <label htmlFor="login-password">Password</label>
                        <input
                            id="login-password"
                           
                            type={showPasswords ? "text" : "password"}
                            autoComplete="current-password"
                            placeholder="Enter your password"
                            value={password}
                            onChange={(event) => {
                                setPassword(event.target.value);
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

                    {error && <p className="auth-alert" role="alert">{error}</p>}

                    <button className="auth-submit" type="submit" disabled={loading}>
                        {loading ? "Signing in..." : "Sign in"}
                    </button>
                </form>

                <p className="auth-switch">
                    Don't have an account? <Link to="/register">Create an account</Link>
                </p>
                <p className="auth-footnote">Secure access for service desk users and administrators.</p>
            </section>
        </main>
    );
}

export default Login;

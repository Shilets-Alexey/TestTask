import { useState } from "react";
import { useNavigate } from "react-router-dom";

function Login() {
    // state variables for email and passwords
    const [email, setEmail] = useState<string>("");
    const [password, setPassword] = useState<string>("");
    const [rememberme, setRememberme] = useState<boolean>(false);
    // state variable for error messages
    const [error, setError] = useState<string>("");
    const navigate = useNavigate();

    // handle change events for input fields
    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        if (name === "email") setEmail(value);
        if (name === "password") setPassword(value);
        if (name === "rememberme") setRememberme(e.target.checked);
    };

    const handleRegisterClick = () => {
        navigate("/Account/Register");
    }

    // handle submit event for the form
    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        // validate email and passwords
        if (!email || !password) {
            setError("Please fill in all fields.");
        } else {
            // clear error message
            setError("");
            let loginurl = "";
            if (rememberme == true)
                loginurl = "/users/login?useCookies=true";
            else
                loginurl = "/users/login?useSessionCookies=true";
            // post data to the loginurl api
            fetch(loginurl, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    email: email,
                    password: password,
                }),
            })

                .then((data) => {
                    // handle success or error from the server
                    if (data.ok) {
                        window.location.href = '/';
                    }
                    else
                        setError("Error Logging In.");

                })
                .catch((error) => {
                    // handle network error
                    console.error(error);
                    setError("Error Logging in.");
                });
        }
    };

    return (
        <div className="containerbox">
            <h3>Login</h3>
            <form onSubmit={handleSubmit}>
                <div>
                    <label className="forminput" htmlFor="email">Email:</label>
                </div>
                <div>
                    <input
                        type="email"
                        id="email"
                        name="email"
                        value={email}
                        onChange={handleChange}
                    />
                </div>
                <div>
                    <label htmlFor="password">Password:</label>
                </div>
                <div>
                    <input
                        type="password"
                        id="password"
                        name="password"
                        value={password}
                        onChange={handleChange}
                    />
                </div>
                <div>
                    <input
                        type="checkbox"
                        id="rememberme"
                        name="rememberme"
                        checked={rememberme}
                        onChange={handleChange} /><span>Remember Me</span>
                </div>
                <div>
                    <button className="simple-btn" type="submit">Login</button>
                </div>
                <div>
                    <button className="simple-btn" onClick={handleRegisterClick}>Register</button>
                </div>
            </form>
            {error && <p className="error">{error}</p>}
        </div>
    );
}

export default Login;
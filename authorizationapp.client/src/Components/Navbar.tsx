import { NavLink } from 'react-router-dom';
import { useEffect, useState } from "react";
import '../CSS/Navbar.css';

const Navbar = () => {
    // state variable autorization
    const [autorized, setAuthorized] = useState<boolean>()
    useEffect(() => {
        checkAutorization();
    }, []);

    async function checkAutorization() {
        try {
            // make the fetch request
            const response = await fetch("/users/pingauth", {
                method: "GET",
            });

            // check the status code
            if (response.status == 200) {
                const data = await response.json();
                if (data && data.validationResult) {
                    if (data.validationResult.isValid) {
                        setAuthorized(data.result.isAutorized);
                    } else {
                        if (data.validationResult.errors) {
                            const keys = Object.keys(data.validationResult.errors);
                            if (keys.length > 0) {
                                alert(data.validationResult.errors[keys[0]]);
                            }
                        }
                    }
                }
                return; 
            }
        } catch(error) {
            console.error(error);
        }
    }

    return (
        <nav className="navbar">
            <div className="container">
                <div className="nav-elements">
                    <ul>
                        <div className="nav-elemens-block">
                            <li>
                                <NavLink to="/">Wheather</NavLink>
                            </li>
                            <li>
                                <NavLink to="/home">Home</NavLink>
                            </li>
                        </div>  
                        { 
                            autorized
                                ?
                                <div className="nav-elemens-block">
                                    <li>
                                        <NavLink to="/Account/Manage">Profile</NavLink>
                                    </li>
                                    <li>
                                        <NavLink to="/users/logout">Logout</NavLink>
                                    </li>
                                </div>
                                :
                                <div className="nav-elemens-block">
                                    <li>
                                        <NavLink to="/Account/Login">Login</NavLink>
                                    </li>
                                    <li>
                                        <NavLink to="/Account/Register">Register</NavLink>
                                    </li>
                                </div>
                        }
                    </ul>
                </div>
            </div>
        </nav>
    )
}

export default Navbar;

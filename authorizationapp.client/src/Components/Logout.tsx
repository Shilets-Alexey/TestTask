
import { useState } from 'react';

function Logout() {
    // state error variabl 
    const [error, setError] = useState<string>("");
    // make the fetch request
    fetch("/logout", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
    })
    .then((data) => {
        // handle success or error from the server
        console.log(data);
        if (data.ok) {
            setError("Successful Logout.");
            window.location.href = '/';
        }
        else
            setError("Error logout.");

    })
    .catch((error) => {
        console.error(error);
        setError("Error logout.");
    });

    return (error)
}

export default Logout;
function Logout() {
    // make the fetch request
    fetch("/users/logout", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
    })
    .then((data) => {
        // handle success or error from the server
        if (data.ok) {
            window.location.href = '/';
        }
    })
    .catch((error) => {
        console.error(error);
    });

    return
}

export default Logout;
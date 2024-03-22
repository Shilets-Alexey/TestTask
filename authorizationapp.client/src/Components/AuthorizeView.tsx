import React, { useState, useEffect, createContext } from 'react';
import { Navigate } from 'react-router-dom';


const UserContext = createContext({});

interface User {
    email: string;
}


function AuthorizeView(props: { children: React.ReactNode }) {

    // state variable autorization
    const [authorized, setAuthorized] = useState<boolean>(false);
    // state variable for loading
    const [loading, setLoading] = useState<boolean>(true);
    let emptyuser: User = { email: "" };

    const [user, setUser] = useState(emptyuser);


    useEffect(() => {
        // Get the cookie value
        let retryCount = 0; // initialize the retry count
        const maxRetries = 3; // set the maximum number of retries
        const delay = 100;
        // define a delay function that returns a promise
        function wait(delay: number) {
            return new Promise((resolve) => setTimeout(resolve, delay));
        }

        // define a fetch function that retries until status 200 or 401
        async function fetchWithRetry(url: string, options: any) {
            try {
                // make the fetch request
                const response = await fetch(url, options);

                // check the status code
                if (response.status == 200) {
                    console.log("Authorized");
                    const resp = await response.json();
                    
                    setAuthorized(resp.isAuthenticated);
                    setUser({ email: resp.email });
                    return response; // return the response
                } else if (response.status == 401) {
                    console.log("Unauthorized");
                    return response; // return the response
                } else {
                    // throw an error to trigger the catch block
                    throw new Error("" + response.status);
                }
            } catch (error) {
                // increment the retry count
                retryCount++;
                // check if the retry limit is reached
                if (retryCount > maxRetries) {
                    // stop retrying and rethrow the error
                    throw error;
                } else {
                    // wait for some time and retry
                    await wait(delay);
                    return fetchWithRetry(url, options);
                }
            }
        }

        // call the fetch function with retry logic
        fetchWithRetry("/users/pingauth", {
            method: "GET",
        })
            .catch((error) => {
                // handle the final error
                console.log(error.message);
            })
            .finally(() => {
                setLoading(false);  // set loading to false when the fetch is done
            });
    }, []);


    if (loading) {
        return (
            <>
                <p>Loading...</p>
            </>
        );
    }
    else {
        if (authorized && !loading) {
            return (
                <>
                    <UserContext.Provider value={user}>{props.children}</UserContext.Provider>
                </>
            );
        } else {
            return (
                <>
                    <Navigate to="/Account/Login" />
                </>
            )
        }
    }

}

export default AuthorizeView;
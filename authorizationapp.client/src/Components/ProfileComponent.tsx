import { useEffect, useState } from "react";
import { User } from "./UsersInterface";

const ProfileComponent = () => {
    const emptyUser: User = { userName: "" };
    // state error variable
    const [error, setError] = useState<string>("");
    // state file url variable
    const [file, setFile] = useState<string>();
    // state current user variable
    const [user, setUser] = useState(emptyUser);
    useEffect(() => {
        GetCurrenUserData();
    }, [])

    async function handleChange(e) {
        if (e.target.files[0]) {
            //validate file size
            if (e.target.files[0].size / 1024 > 20) {
                setError('File size must be less then 20kb');
                return;
            }
            try {
                const form = new FormData();
                form.append('file', e.target.files[0]);
                // fetch request
                const response = await fetch('/users/' + user.id + "/photo", {
                    method: "PATCH", headers: { "contentType": "multipart/formdata" }, body: form
                });
                // check the status code
                if (response.ok) {
                    setError("")
                    //set filer url
                    setFile(URL.createObjectURL(e.target.files[0]));
                } else {
                    //set first from server
                    const errorInfo = await response.json();
                    if (errorInfo.errors) {
                        const keys = Object.keys(errorInfo.errors);
                        if (keys.length > 0) {
                            setError(errorInfo.errors[keys[0]]);
                        }
                    }
                    
                }
            } catch (error) {
                console.log(error);
            }
            
            
        }
    }

    async function GetCurrenUserData() {
            // make the fetch request
        const response = await fetch("/users/current", { method: "GET" });

        // check the status code
        if (response.status == 200) {
            const data = await response.json();
            setUser(data);
            setFile("data:" + data?.imgType + ";base64," + data?.imgData);
        } else {
            throw new Error("" + response.status);
        }
    } 

    return (
        <div>
            <div>
                <label className="form-label">Name</label>
                <input className="form-control" type="text" value={user?.userName} disabled={true} />
            </div>
            <div className="App">
                <h2>Add Image:</h2>
                <img className= "form-img" src={file} width="240px" height="240px" />
            </div>
            <div>
                <input className="form-control" type="file" onChange={handleChange} accept=".png,.jpeg,.png" />
                {error && <p className="error">{error}</p>}
            </div>
        </div>
    );
}

export default ProfileComponent
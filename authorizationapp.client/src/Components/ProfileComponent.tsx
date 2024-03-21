import { useEffect, useState } from "react";
import { User } from "./UsersInterface";

const ProfileComponent = () => {
    let emptyUser: User = { userName: "" };
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
                let form = new FormData();
                form.append('File', e.target.files[0]);
                // fetch request
                var response = await fetch('/updatePhoto?userId=' + user.id, {
                    method: "PATCH", headers: { "contentType": "multipart/formdata" }, body: form
                });
                // check the status code
                if (response.ok) {
                    setError("")
                    //set filer url
                    setFile(URL.createObjectURL(e.target.files[0]));
                } else {
                    //set first from server
                    var errorInfo = await response.json();
                    if (errorInfo.errors) {
                        let keys = Object.keys(errorInfo.errors);
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
        try {
            // make the fetch request
            let response = await fetch("/currentUser", { method: "GET" });

            // check the status code
            if (response.status == 200) {
                let data = await response.json();
                setUser(data);
                setFile("data:" + data?.imageType + ";base64," + data?.profileImg);
            } else {
                throw new Error("" + response.status);
            }
        } catch (error) {
            throw error;
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
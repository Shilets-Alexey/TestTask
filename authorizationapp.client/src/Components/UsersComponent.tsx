import { useEffect, useState } from "react";
import { User } from "./UsersInterface";

const UsersComponent = () => {
    // state variable for users
    const [users, setUsers] = useState<User[]>()
    useEffect(() => {
        populateUsersData();
    }, [])
    
    async function getUsers() {

        // make the fetch request
        const response = await fetch("/users");
            
        // check the status code
        if (response.status == 200) {
            const data = await response.json();
            if (data && data.validationResult) {
                if (data.validationResult.isValid) {
                    setUsers(data.result);
                } else {
                    if (data.validationResult.errors) {
                        const keys = Object.keys(data.validationResult.errors);
                        if (keys.length > 0) {
                            alert(data.validationResult.errors[keys[0]]);
                        }
                    }
                }
            } else {
                throw new Error("Invalid response data" + response.status);
            }
        } else {
            throw new Error("" + response.status);
        }
    }

    const contents = users == undefined
        ? <p>Thera no users in system</p>
        :
        <div>
            <div></div>
            <table className="table table-striped" aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>User name</th>
                        <th>Roles</th>
                        <th>Last login date</th>
                        <th>Succeeded logins count</th>
                        <th>photo</th>
                        <th>actions</th>
                    </tr>
                </thead>
                <tbody>
                    {users.map(user => <tr key={user.userName}>
                        <td>{user.userName}</td>
                        <td>{user.userRoles}</td>
                        <td>{user.lastLoginDate}</td>
                        <td>{user.succeededLoginsCount}</td>
                        <td>
                            {
                                user?.imgData 
                                    ? <img className="form-img" src={"data:" + user?.imgType + ";base64," + user?.imgData} />
                                    : ""
                            }
                        </td>  
                        <td>
                            {user.isBtnVisible ? <button className="red-btn" onClick={() => deleteUser(user)}> Delete </button> : ""}
                            {user.isBtnVisible ? <button className="blue-btn" onClick={() => setAdminRights(user)}> Grant admin rights </button> : ""}
                        </td>
                    </tr>
                    )}
                </tbody>
            </table>
        </div>;

    async function deleteUser(user: User) {
        try {
            // make the fetch request
            const response = await fetch("admins/" + user.id, { method: "DELETE" });
            if (response.ok) {
                const data = await response.json();
                if (data && data.isValid) {
                    // remove users from client
                    const usersNewList = users?.filter(x => x.id !== user.id);
                    setUsers(usersNewList);
                } else {
                    if (data.errors) {
                        const keys = Object.keys(data.errors);
                        if (keys.length > 0) {
                            alert(data.errors[keys[0]]);
                        }
                    }
                }
            }
            else {
                //set first from server
                const errorInfo = await response.json();
                if (errorInfo.errors) {
                    const keys = Object.keys(errorInfo.errors);
                    if (keys.length > 0) {
                        alert(errorInfo.errors[keys[0]]);
                    }
                }
            }
        }catch(error) {
            console.log(error)
        }
        
        
    }

    async function setAdminRights(user: User) {
        // make the fetch request
        const response = await fetch("admins/" + user.id + "/role/" + "Admin", { method: "PATCH" })
        if (response.ok) {
            const data = await response.json();
            if (data && data.isValid) {
                // remove users from client
                getUsers().catch((error) => { console.log(error.message) });
            } else {
                if (data.errors) {
                    const keys = Object.keys(data.errors);
                    if (keys.length > 0) {
                        alert(data.errors[keys[0]]);
                    }
                }
            }
        }
        else {
            //set first from server
            const errorInfo = await response.json();
            if (errorInfo.errors) {
                const keys = Object.keys(errorInfo.errors);
                if (keys.length > 0) {
                    alert(errorInfo.errors[keys[0]]);
                }
            }
        }
        
    }

    async function populateUsersData() {
        //set users data
        getUsers().catch((error) => { console.log(error.message) });
    }

    return (
        <div>
            <h1 id="tabelLabel">ListOfUsers</h1>
            {contents}
        </div>
    );

}

export default UsersComponent;
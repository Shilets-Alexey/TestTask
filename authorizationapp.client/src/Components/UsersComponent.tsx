import { useEffect, useState } from "react";
import { User } from "./UsersInterface";

const UsersComponent = () => {
    // state variable for users
    const [users, setUsers] = useState<User[]>()
    useEffect(() => {
        populateUsersData();
    }, [])
    
    async function getUsers() {
        try {
            // make the fetch request
            let response = await fetch("/users", { method: "GET" });
                
            // check the status code
            if (response.status == 200) {
                let data = await response.json();
                // set users dta
                setUsers(data);
            } else {
                throw new Error("" + response.status);
            }
        } catch (error) {
            throw error;
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
                        <th>UserName</th>
                        <th>userRoles</th>
                        <th>lastLogin</th>
                        <th>succesLogins</th>
                        <th>profileImg</th>
                        <th>actions</th>
                    </tr>
                </thead>
                <tbody>
                    {users.map(user => <tr key={user.userName}>
                        <td>{user.userName}</td>
                        <td>{user.userRoles}</td>
                        <td>{user.succesLogins}</td>
                        <td>{user.lastLogin}</td>
                        <td>
                            {
                                user?.profileImg 
                                    ? <img className="form-img" src={"data:" + user?.imageType + ";base64," + user?.profileImg} />
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
            var response = await fetch("/deleteUser?userId=" + user.id, { method: "DELETE" });
            if (response.ok) {
                // remove users from client
                let newUsersKust = users?.filter(x => x.id !== user.id);
                setUsers(newUsersKust);
            }
            else {
                //set first from server
                var errorInfo = await response.json();
                if (errorInfo.errors) {
                    let keys = Object.keys(errorInfo.errors);
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
        var response = await fetch("/setAdminRights?userId=" + user.id, { method: "POST" })
        if (response.ok) {
            // get new users information
            getUsers().catch((error) => { console.log(error.message) });
        }
        else {
            //set first from server
            var errorInfo = await response.json();
            if (errorInfo.errors) {
                let keys = Object.keys(errorInfo.errors);
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
export interface User {
    id: string,
    userName: string,
    userRoles: string,
    succesLogins: number,
    lastLogin: string,
    profileImg: string,
    isBtnVisible: boolean,
    deleteBtn: JSX.Element,
    editBtn: JSX.Element,
    imageType: string
}
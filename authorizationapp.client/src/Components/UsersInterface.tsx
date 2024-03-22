export interface User {
    id: string,
    userName: string,
    userRoles: string,
    succeededLoginsCount: number,
    lastLoginDate: string,
    imgData: string,
    imgType: string,
    isBtnVisible: boolean,
    deleteBtn: JSX.Element,
    editBtn: JSX.Element
    
}
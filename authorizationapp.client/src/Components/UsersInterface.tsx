export interface User {
    id: string,
    userName: string,
    userRoles: string,
    succeededLoginsCount: number,
    lastLoginDate: string,
    imgData: string,
    imgType: string,
    isBtnVisible: boolean,
    canDelete: JSX.Element,
    canGrantRole: JSX.Element
    
}
export interface UserProfileModel {
  firstName: string
  lastName: string

  email: string
  phone?: string

  isAdministrator: string
}

export interface UserAuthModel {
  accessToken: string
  refreshToken: string
}

export interface UserVerifyModel {
  isAuthorized: string
  isAdministrator: string
}


export interface UserRegisterRequest {
  firstName: string
  lastName: string

  email: string
  phone?: string

  password: string
}

export interface UserLoginRequest {
  email: string
  password: string
}

export interface UserRefreshRequest {
  refreshToken: string
}

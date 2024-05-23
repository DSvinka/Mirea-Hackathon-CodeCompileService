import { defineStore } from 'pinia'

import {UserAuthModel, UserLoginRequest, UserProfileModel, UserVerifyModel} from "../api/models/user-models";
import {
  Auth,
  getAccessToken,
  deleteRefreshToken,
  deleteAccessToken,
  getAuthHeader,
  setAccessToken,
  setRefreshToken
} from "../api"
import {ErrorModel} from "../api/models/error-models";

interface AuthState {
  profile: UserProfileModel
  isLoggedIn: boolean
}

export const useAuthStore = defineStore("auth", {
  state: (): AuthState => ({
    profile: null,
    isLoggedIn: false
  }),

  getters: {
    getProfile(state: AuthState): UserProfileModel{
      return state.profile
    }
  },

  actions: {
    async getProfile() {
      try {
        const data = await Auth.get("/profile", {headers: getAuthHeader()})
        this.profile = data.data
      }
      catch (error) {
        alert(error)
        console.log(error)
      }
    },

    async verify() {
      try {
        const response = await Auth.get<UserVerifyModel | ErrorModel>("/verify", {headers: getAuthHeader()})
        if (response.status === 200) {
          const data = (response.data as UserVerifyModel);
          this.isLoggedIn = data.isAuthorized

          if (data.isAuthorized) {
            await this.logout();
          }

        } else {
          alert((response.data as ErrorModel).message)
        }
      }
      catch (error) {
        alert(error)
        console.log(error)
      }
    },


    // TODO: Нужна система уведомлений, чтобы не на Alert всё строить.
    async login(model: UserLoginRequest) {
      try {
        const response = await Auth.post<UserAuthModel | ErrorModel>("/login", model)
        if (response.status === 200) {
          const data = (response.data as UserAuthModel);
          setAccessToken(data.accessToken)
          setRefreshToken(data.refreshToken)
        } else {
          alert((response.data as ErrorModel).message)
        }
      }
      catch (error) {
        alert(error)
        console.log(error)
      }
    },

    async register(model: UserLoginRequest) {
      try {
        const response = await Auth.post<void | ErrorModel>("/register", model)
        if (response.status === 200) {
          alert("Теперь авторизируйтесь!")
        } else {
          alert((response.data as ErrorModel).message)
        }
      }
      catch (error) {
        alert(error)
        console.log(error)
      }
    },


    async logout() {
      deleteAccessToken();
      deleteRefreshToken();

      this.isLoggedIn = false
      this.profile = null;
    },


    checkLoggedIn(){
      if ( getAccessToken() ){
        this.isLoggedIn = true
      }
    }
  },
})

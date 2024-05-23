import axios from "axios";

export const config = {
  apiUrl: "https://compile.dsvinka.ru/api/services",
  wsUrl: "https://compile.dsvinka.ru/api/ws",
}


export const Auth = axios.create({
  baseURL: `${config.apiUrl}/auth`,
  withCredentials: true,
  timeout: 5000,
  headers: {
    'Content-Type': 'application/json'
  },
})

export const Docker = axios.create({
  baseURL: `${config.apiUrl}/docker`,
  withCredentials: true,
  timeout: 5000,
  headers: {
    'Content-Type': 'application/json'
  },
})


export function getAuthHeader(opt_headers = {}) {
  const access_token = localStorage.getItem('accessToken')
  const auth_header = {'Authorization': `Bearer ${access_token}`}

  return {...opt_headers, ...auth_header}
}


export function getAccessToken() {
  return localStorage.getItem('accessToken');
}

export function getRefreshToken() {
  return localStorage.getItem('refreshToken');
}

export function deleteAccessToken() {
  localStorage.removeItem('accessToken');
}

export function deleteRefreshToken() {
  localStorage.removeItem('refreshToken');
}


export function setRefreshToken(token) {
  return localStorage.setItem('refreshToken', token);
}

export function setAccessToken(token) {
  return localStorage.setItem('accessToken', token);
}

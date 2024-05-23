export interface ErrorModel {
  message: string
  code?: string

  data?: { [key: string]: string };
}

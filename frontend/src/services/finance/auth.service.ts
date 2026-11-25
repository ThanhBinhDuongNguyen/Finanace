import { http } from "../http";

export type FinanceSession = { username: string; roles: string[]; shops: string[] };
export const authService = {
  me: () => http.get<FinanceSession>("/auth/me").then((response) => response.data),
  login: (payload: { username: string; password: string }) => http.post<FinanceSession>("/auth/login", payload).then((response) => response.data),
  logout: () => http.post("/auth/logout"),
};

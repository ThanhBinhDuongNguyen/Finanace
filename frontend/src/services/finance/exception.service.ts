import { http } from "../http";
import type { ExceptionAction, FinanceExceptionInput } from "./types";

export const exceptionService = {
  create: (payload: FinanceExceptionInput) => http.post("/finance/exceptions", payload).then((response) => response.data),
  requestClosure: (id: string, payload: ExceptionAction) => http.post(`/finance/exceptions/${id}/request-closure`, payload).then((response) => response.data),
  approveClosure: (id: string, payload: ExceptionAction) => http.post(`/finance/exceptions/${id}/approve-closure`, payload).then((response) => response.data),
  reopen: (id: string, payload: ExceptionAction) => http.post(`/finance/exceptions/${id}/reopen`, payload).then((response) => response.data),
  list: () => http.get("/finance/exceptions").then((response) => response.data),
};

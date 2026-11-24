import { http } from "../http";
import type { BankTransactionInput, PayoutInput, ReconciliationResult } from "./types";

export const reconciliationService = {
  createPayout: (payload: PayoutInput) => http.post("/finance/reconciliation/payouts", payload),
  listPayouts: () => http.get<Array<{ id: string; externalId: string; amount: number; shopCode: string; currency: string }>>("/finance/reconciliation/payouts").then((response) => response.data),
  createBankTransaction: (payload: BankTransactionInput) => http.post("/finance/reconciliation/bank-transactions", payload),
  listBankTransactions: () => http.get<Array<{ id: string; externalId: string; amount: number; shopCode: string; currency: string }>>("/finance/reconciliation/bank-transactions").then((response) => response.data),
  createCase: (payload: { payoutId: string; bankTransactionId: string; tolerance: number; evidence: string; proposedBy: string }) => http.post<ReconciliationResult>("/finance/reconciliation/cases", payload).then((response) => response.data),
  confirm: (id: string, reviewer: string) => http.post<ReconciliationResult>(`/finance/reconciliation/cases/${id}/confirm`, reviewer).then((response) => response.data),
  listCases: () => http.get("/finance/reconciliation/cases").then((response) => response.data),
};

import { http } from "../http";
import type { BankTransaction, BankTransactionInput, Payout, PayoutInput, ReconciliationCase, ReconciliationResult } from "./types";

export const reconciliationService = {
  createPayout: (payload: PayoutInput) => http.post("/finance/reconciliation/payouts", payload),
  listPayouts: () => http.get<Payout[]>("/finance/reconciliation/payouts").then((response) => response.data),
  createBankTransaction: (payload: BankTransactionInput) => http.post("/finance/reconciliation/bank-transactions", payload),
  listBankTransactions: () => http.get<BankTransaction[]>("/finance/reconciliation/bank-transactions").then((response) => response.data),
  createCase: (payload: { payoutId: string; bankTransactionId: string; tolerance: number; evidence: string; proposedBy: string; reason: string; allocations: Array<{ importLineId: string; amount: number; orderLineId?: string }> }) => http.post<ReconciliationResult>("/finance/reconciliation/cases", payload).then((response) => response.data),
  confirm: (id: string, reviewer: string) => http.post<ReconciliationResult>(`/finance/reconciliation/cases/${id}/confirm`, JSON.stringify(reviewer), { headers: { "Content-Type": "application/json" } }).then((response) => response.data),
  listCases: () => http.get<ReconciliationCase[]>("/finance/reconciliation/cases").then((response) => response.data),
};

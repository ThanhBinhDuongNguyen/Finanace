import { http } from "../http";
import type { Journal } from "./types";

export type LedgerReport = {
  snapshotAt: string; definitionVersion: string; sourceStatus: "Missing" | "Provisional" | "PostedLedger"; unknownCount: number;
  grossRevenue: number; deductions: number; netRevenue: number; cogs: number; operatingExpense: number; managementProfit: number; receivedCash: number;
  accounts: Array<{ accountCode: string; debit: number; credit: number }>;
  entries: Journal[];
};
export const reportService = {
  ledger: (params: { shopCode: string; currency: string; period: string }) => http.get<LedgerReport>("/finance/reports/ledger", { params }).then((response) => response.data),
  exportJournals: (shopCode: string) => http.get<Blob>("/finance/reports/journals.csv", { params: { shopCode }, responseType: "blob" }).then((response) => response.data),
};

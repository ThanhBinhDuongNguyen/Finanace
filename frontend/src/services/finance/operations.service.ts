import { http } from "../http";

export type OperationsSummary = { importBatchCount: number; disputedCaseCount: number; openExceptionCount: number; postedJournalCount: number };

export const operationsService = {
  summary: () => http.get<OperationsSummary>("/finance/operations/summary").then((response) => response.data),
};

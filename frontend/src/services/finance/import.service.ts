import { http } from "../http";
import type { ImportBatch, ImportBatchInput, ImportBatchResult, ImportLine } from "./types";

export const importService = {
  create: (payload: ImportBatchInput) => http.post<ImportBatchResult>("/finance/import-batches", payload).then((response) => response.data),
  preview: (payload: ImportBatchInput) => http.post<{ lines: ImportLine[]; validLines: number; quarantinedLines: number; isDuplicate: boolean }>("/finance/import-batches/preview", payload).then((response) => response.data),
  list: () => http.get<ImportBatch[]>("/finance/import-batches").then((response) => response.data),
};

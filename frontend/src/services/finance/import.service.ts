import { http } from "../http";
import type { ImportBatchInput, ImportBatchResult } from "./types";

export const importService = {
  create: (payload: ImportBatchInput) => http.post<ImportBatchResult>("/finance/import-batches", payload).then((response) => response.data),
  list: () => http.get("/finance/import-batches").then((response) => response.data),
};

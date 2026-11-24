import { http } from "../http";
import type { ImportSourceType } from "./types";

export const classificationService = {
  addMapping: (payload: { sourceType: ImportSourceType; sourceBusinessType: string; targetCategory: string; version: number; effectiveFrom: string; effectiveTo?: string; createdBy: string }) => http.post("/finance/classification/mappings", payload),
  classifyBatch: (id: string, payload: { effectiveOn: string; actor: string }) => http.post(`/finance/classification/batches/${id}/classify`, payload).then((response) => response.data),
  listMappings: () => http.get("/finance/classification/mappings").then((response) => response.data),
  listTransactions: () => http.get("/finance/classification/transactions").then((response) => response.data),
};

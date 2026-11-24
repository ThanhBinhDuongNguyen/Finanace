import { http } from "../http";
import type { JournalInput } from "./types";

export const ledgerService = {
  post: (payload: JournalInput) => http.post("/finance/ledger/journals", payload).then((response) => response.data),
  reverse: (id: string, payload: { period: string; postedBy: string }) => http.post(`/finance/ledger/journals/${id}/reverse`, payload).then((response) => response.data),
  closePeriod: (payload: { shopCode: string; period: string; closedBy: string }) => http.post("/finance/ledger/periods/close", payload),
  list: () => http.get("/finance/ledger/journals").then((response) => response.data),
};

import { http } from "../http";

export type OrderSnapshot = { id: string; externalId: string; orderId: string; sku: string; shopCode: string; currency: string; quantity: number; netAmount: number };
export type RefundRequest = { id: string; orderLineId: string; amount: number; sourceRequestId: string; proposedBy?: string; requestedBy?: string; approvedBy?: string; status: string };
export type DocumentRevision = { revisionId: string; fileName: string; status: string; version: number; uploadedBy: string };
export type DocumentRecord = { id: string; externalId: string; kind: string; revisions: DocumentRevision[] };
export type ExtensionOperation = "orders" | "refunds/requests" | "refunds/external-confirmations" | "costs/policies" | "costs/layers" | "costs/assessments" | "returns/cost-adjustments" | "failed-delivery-fees";
export type PayoutAccounts = {
  history: Array<{ id: string; accountReference: string; activatedAt: string; deactivatedAt?: string }>;
  changes: Array<{ id: string; sourceRequestId: string; newAccountReference: string; proposedBy: string; status: string; independentChannelEvidence: string }>;
  verificationStatus: string;
};
export const extensionService = {
  payoutAccounts: (shopCode: string) => http.get<PayoutAccounts>("/finance/extensions/payout-accounts", { params: { shopCode } }).then((response) => response.data),
  proposePayoutAccount: (payload: { shopCode: string; sourceRequestId: string; newAccountReference: string; independentChannelEvidence: string; reason: string }) => http.post("/finance/extensions/payout-accounts/changes", payload),
  approvePayoutAccount: (id: string, evidence: string) => http.post(`/finance/extensions/payout-accounts/changes/${id}/approve`, { evidence }),
  orders: (shopCode: string) => http.get<OrderSnapshot[]>("/finance/extensions/orders", { params: { shopCode } }).then((response) => response.data),
  refunds: (shopCode: string) => http.get("/finance/extensions/refunds", { params: { shopCode } }).then((response) => response.data),
  refundRequests: (shopCode: string) => http.get<RefundRequest[]>("/finance/extensions/refunds/requests", { params: { shopCode } }).then((response) => response.data),
  costs: (shopCode: string) => http.get("/finance/extensions/costs", { params: { shopCode } }).then((response) => response.data),
  returnsAndFees: (shopCode: string) => http.get("/finance/extensions/returns-and-fees", { params: { shopCode } }).then((response) => response.data),
  documents: (shopCode: string) => http.get<DocumentRecord[]>("/finance/extensions/documents", { params: { shopCode } }).then((response) => response.data),
  audit: (shopCode: string) => http.get("/finance/extensions/audit", { params: { shopCode } }).then((response) => response.data),
  execute: (operation: ExtensionOperation, payload: Record<string, unknown>) => http.post(`/finance/extensions/${operation}`, payload).then((response) => response.data),
  approveRefund: (id: string, evidence: string) => http.post(`/finance/extensions/refunds/requests/${id}/approve`, { evidence }),
  uploadDocument: (payload: { shopCode: string; externalId: string; kind: string; sourceId: string; fileName: string; contentBase64: string; reason: string }) => http.post("/finance/extensions/documents", payload).then((response) => response.data),
  reviewDocument: (id: string, payload: { approve: boolean; note: string }) => http.post(`/finance/extensions/documents/revisions/${id}/review`, payload),
  downloadDocument: (id: string) => http.get<Blob>(`/finance/extensions/documents/revisions/${id}/content`, { responseType: "blob" }).then((response) => response.data),
};

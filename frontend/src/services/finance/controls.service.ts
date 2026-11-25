import { http } from "../http";

export type TolerancePolicy = { id: string; currency: string; amount: number; effectiveFrom: string; effectiveTo: string; approvedBy: string; evidence: string };
export type ReopenRequest = { id: string; shopCode: string; period: string; proposedBy: string; reason: string; evidence: string; approvedBy?: string; createdAt: string };
export type SourceAuthority = { id: string; shopCode: string; mode: "Local" | "Connected"; version: number; approvedBy: string };
export type SourceSwitchRequest = { id: string; shopCode: string; targetMode: "Local" | "Connected"; expectedVersion: number; proposedBy: string; checkpoint: string; mappingManifest: string; openWorkManifest: string; approvedBy?: string };
export type SourceState = { authorities: SourceAuthority[]; requests: SourceSwitchRequest[] };

export const controlsService = {
  policies: () => http.get<TolerancePolicy[]>("/finance/controls/policies").then((response) => response.data),
  addPolicy: (payload: Pick<TolerancePolicy, "currency" | "amount" | "effectiveFrom" | "effectiveTo" | "evidence">) => http.post<TolerancePolicy>("/finance/controls/policies", payload).then((response) => response.data),
  reopenRequests: () => http.get<ReopenRequest[]>("/finance/controls/reopen-requests").then((response) => response.data),
  requestReopen: (payload: Pick<ReopenRequest, "shopCode" | "period" | "reason" | "evidence">) => http.post<ReopenRequest>("/finance/controls/reopen-requests", payload).then((response) => response.data),
  approveReopen: (id: string) => http.post<ReopenRequest>(`/finance/controls/reopen-requests/${id}/approve`, {}).then((response) => response.data),
  sources: () => http.get<SourceState>("/finance/sources").then((response) => response.data),
  requestSwitch: (payload: { shopCode: string; targetMode: "Local" | "Connected"; expectedVersion: number; checkpoint: string; mapping: Record<string, string> }) => http.post<SourceSwitchRequest>("/finance/sources/switch-requests", payload).then((response) => response.data),
  approveSwitch: (id: string) => http.post<SourceAuthority>(`/finance/sources/switch-requests/${id}/approve`, {}).then((response) => response.data),
};

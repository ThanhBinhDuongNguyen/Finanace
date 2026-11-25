export type ImportSourceType = "MarketplaceStatement" | "BankStatement";
export type ImportBatchInput = { sourceType: ImportSourceType; shopCode: string; period: string; currency: string; importedBy: string; lines: Array<{ externalId?: string; amount: number; occurredOn: string; businessType?: string; rawData?: string }> };
export type ImportBatchResult = { id: string; status: string; checksum: string; validLines: number; quarantinedLines: number; isDuplicate: boolean };

export type PayoutInput = { externalId: string; shopCode: string; currency: string; amount: number; payoutOn: string };
export type BankTransactionInput = { externalId: string; shopCode: string; currency: string; amount: number; creditedOn: string };
export type ReconciliationResult = { id: string; status: "Candidate" | "FullyMatched" | "Disputed"; difference: number; tolerance: number; reviewedBy?: string };

export type ExceptionAction = { actor: string; note: string; evidence: string; outcome?: "Verified" | "AcceptVariance" };
export type FinanceExceptionInput = { referenceType: string; referenceId: string; shopCode: string; reasonCode: string; amount?: number; owner: string; dueOn: string; evidence: string; createdBy: string };

export type JournalLine = { accountCode: string; debit: number; credit: number };
export type JournalInput = { referenceType: string; referenceId: string; shopCode: string; currency: string; period: string; postedBy: string; lines: JournalLine[]; reason: string; evidence: string };

export type ImportLine = { id: string; externalId: string; amount: number; occurredOn: string; businessType: string; rawData: string; status: string; validationError?: string };
export type ImportBatch = { id: string; shopCode: string; period: string; currency: string; sourceType: ImportSourceType; checksum: string; lines: ImportLine[] };
export type Payout = PayoutInput & { id: string };
export type BankTransaction = BankTransactionInput & { id: string };
export type ReconciliationCase = ReconciliationResult & { payoutId: string; bankTransactionId: string; evidence: string; proposedBy: string; createdAt: string };
export type FinanceException = FinanceExceptionInput & { id: string; status: "Open" | "WaitingExternal" | "PendingApproval" | "Closed"; activities: Array<{ id: string; action: string; actor: string; note: string; evidence: string; occurredAt: string }> };
export type Journal = JournalInput & { id: string; status: string; reversesJournalEntryId?: string };
export type ClassificationMapping = { id: string; sourceType: ImportSourceType; sourceBusinessType: string; targetCategory: string; version: number; effectiveFrom: string; effectiveTo?: string };
export type ClassifiedTransaction = { id: string; importBatchId: string; importLineId: string; category: string; status: string; classificationMappingId?: string };

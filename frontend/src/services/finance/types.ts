export type ImportSourceType = "MarketplaceStatement" | "BankStatement";
export type ImportBatchInput = { sourceType: ImportSourceType; shopCode: string; period: string; currency: string; importedBy: string; lines: Array<{ externalId?: string; amount: number; occurredOn: string; businessType?: string; rawData?: string }> };
export type ImportBatchResult = { id: string; status: string; checksum: string; validLines: number; quarantinedLines: number; isDuplicate: boolean };

export type PayoutInput = { externalId: string; shopCode: string; currency: string; amount: number; payoutOn: string };
export type BankTransactionInput = { externalId: string; shopCode: string; currency: string; amount: number; creditedOn: string };
export type ReconciliationResult = { id: string; status: "Candidate" | "FullyMatched" | "Disputed"; difference: number; tolerance: number; reviewedBy?: string };

export type ExceptionAction = { actor: string; note: string; evidence: string };
export type FinanceExceptionInput = { referenceType: string; referenceId: string; shopCode: string; reasonCode: string; amount?: number; owner: string; dueOn: string; evidence: string; createdBy: string };

export type JournalLine = { accountCode: string; debit: number; credit: number };
export type JournalInput = { referenceType: string; referenceId: string; shopCode: string; currency: string; period: string; postedBy: string; lines: JournalLine[] };

"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { classificationService } from "../../../services/finance/classification.service";
import { exceptionService } from "../../../services/finance/exception.service";
import { importService } from "../../../services/finance/import.service";
import { ledgerService } from "../../../services/finance/ledger.service";
import { reconciliationService } from "../../../services/finance/reconciliation.service";

const button = "rounded-md bg-[#17375e] px-3 py-2 text-sm font-semibold text-white disabled:opacity-50";
const input = "rounded-md border border-slate-300 px-3 py-2 text-sm";
const demoCsv = "SETTLE-001,1000000,2026-12-12,Revenue\nFEE-001,-10000,2026-12-12,PlatformFee";
type Item = { id: string; status?: string; externalId?: string; amount?: number; reasonCode?: string; shopCode?: string; period?: string; difference?: number };

export function FinanceOperations() {
  const query = useQueryClient();
  const [csv, setCsv] = useState(demoCsv);
  const [message, setMessage] = useState("");
  const [payoutId, setPayoutId] = useState("");
  const [bankId, setBankId] = useState("");
  const [batchId, setBatchId] = useState("");
  const refresh = (...keys: string[]) => Promise.all(keys.map((key) => query.invalidateQueries({ queryKey: ["finance", key] })));
  const imports = useQuery({ queryKey: ["finance", "imports"], queryFn: importService.list });
  const payouts = useQuery({ queryKey: ["finance", "payouts"], queryFn: reconciliationService.listPayouts });
  const banks = useQuery({ queryKey: ["finance", "banks"], queryFn: reconciliationService.listBankTransactions });
  const cases = useQuery({ queryKey: ["finance", "cases"], queryFn: reconciliationService.listCases });
  const exceptions = useQuery({ queryKey: ["finance", "exceptions"], queryFn: exceptionService.list });
  const journals = useQuery({ queryKey: ["finance", "journals"], queryFn: ledgerService.list });
  const mappings = useQuery({ queryKey: ["finance", "mappings"], queryFn: classificationService.listMappings });
  const importBatch = useMutation({ mutationFn: () => importService.create({ sourceType: "MarketplaceStatement", shopCode: "TIKTOK-HCM", period: "2026-12-01", currency: "VND", importedBy: "finance.maker", lines: csv.split("\n").filter(Boolean).map((rawData) => { const [externalId, amount, occurredOn, businessType] = rawData.split(","); return { externalId, amount: Number(amount), occurredOn, businessType, rawData }; }) }), onSuccess: async (data) => { setMessage(data.isDuplicate ? "Batch đã tồn tại." : `Đã lưu ${data.validLines} dòng hợp lệ, ${data.quarantinedLines} quarantine.`); await refresh("imports"); } });
  const addPayout = useMutation({ mutationFn: () => reconciliationService.createPayout({ externalId: `PAY-${Date.now()}`, shopCode: "TIKTOK-HCM", currency: "VND", amount: 1_000_000, payoutOn: "2026-12-20" }), onSuccess: () => refresh("payouts") });
  const addBank = useMutation({ mutationFn: () => reconciliationService.createBankTransaction({ externalId: `BANK-${Date.now()}`, shopCode: "TIKTOK-HCM", currency: "VND", amount: 990_000, creditedOn: "2026-12-21" }), onSuccess: () => refresh("banks") });
  const createCase = useMutation({ mutationFn: () => reconciliationService.createCase({ payoutId, bankTransactionId: bankId, tolerance: 0, evidence: "bank-statement", proposedBy: "finance.maker" }), onSuccess: () => refresh("cases") });
  const confirm = useMutation({ mutationFn: (id: string) => reconciliationService.confirm(id, "finance.checker"), onSuccess: () => refresh("cases") });
  const actException = useMutation({ mutationFn: ({ id, action }: { id: string; action: string }) => action === "Open" ? exceptionService.requestClosure(id, { actor: "finance.maker", note: "Đã xử lý", evidence: "ticket" }) : action === "PendingApproval" ? exceptionService.approveClosure(id, { actor: "finance.checker", note: "Đã kiểm tra", evidence: "verified" }) : exceptionService.reopen(id, { actor: "finance.owner", note: "Có bằng chứng mới", evidence: "new-evidence" }), onSuccess: () => refresh("exceptions") });
  const postJournal = useMutation({ mutationFn: () => ledgerService.post({ referenceType: "Settlement", referenceId: `SETTLE-${Date.now()}`, shopCode: "TIKTOK-HCM", currency: "VND", period: "2026-12-01", postedBy: "finance.poster", lines: [{ accountCode: "112", debit: 1_000_000, credit: 0 }, { accountCode: "511", debit: 0, credit: 1_000_000 }] }), onSuccess: () => refresh("journals") });
  const createMapping = useMutation({ mutationFn: () => classificationService.addMapping({ sourceType: "MarketplaceStatement", sourceBusinessType: "Revenue", targetCategory: "NetRevenue", version: 1, effectiveFrom: "2026-01-01", createdBy: "finance.admin" }), onSuccess: () => refresh("mappings") });
  const classify = useMutation({ mutationFn: () => classificationService.classifyBatch(batchId, { effectiveOn: "2026-12-12", actor: "finance.maker" }), onSuccess: async (data) => { setMessage(`Classified: ${data.classified}; Unmapped: ${data.unmapped}`); await refresh("exceptions"); } });
  const rows = (data: unknown) => (Array.isArray(data) ? data : []) as Item[];
  const allImports = rows(imports.data), allPayouts = rows(payouts.data), allBanks = rows(banks.data), allCases = rows(cases.data), allExceptions = rows(exceptions.data), allJournals = rows(journals.data);
  return <div className="space-y-5">
    <div className="grid gap-3 sm:grid-cols-4">{[["Batch", allImports.length], ["Đối soát", allCases.length], ["Ngoại lệ mở", allExceptions.filter((item) => item.status !== "Closed").length], ["Journal", allJournals.length]].map(([label, value]) => <div key={String(label)} className="rounded-lg border border-slate-200 bg-white p-4"><p className="text-sm text-slate-500">{label}</p><p className="text-2xl font-bold text-[#17375e]">{value}</p></div>)}</div>
    {message && <p className="rounded-md bg-blue-50 p-3 text-sm text-blue-900">{message}</p>}
    <section className="rounded-lg border border-slate-200 bg-white p-5"><h2 className="font-semibold">1. Import statement</h2><textarea value={csv} onChange={(event) => setCsv(event.target.value)} className="mt-3 min-h-24 w-full rounded-md border border-slate-300 p-3 font-mono text-sm" /><button type="button" className={`${button} mt-3`} onClick={() => importBatch.mutate()}>Preview & commit</button></section>
    <div className="grid gap-5 xl:grid-cols-2"><section className="rounded-lg border border-slate-200 bg-white p-5"><h2 className="font-semibold">2. Đối soát</h2><div className="mt-3 flex gap-2"><button type="button" className={button} onClick={() => addPayout.mutate()}>+ Payout 1.000.000</button><button type="button" className={button} onClick={() => addBank.mutate()}>+ Bank 990.000</button></div><div className="mt-3 grid gap-2"><select className={input} value={payoutId} onChange={(event) => setPayoutId(event.target.value)}><option value="">Chọn payout</option>{allPayouts.map((item) => <option key={item.id} value={item.id}>{item.externalId} · {item.amount}</option>)}</select><select className={input} value={bankId} onChange={(event) => setBankId(event.target.value)}><option value="">Chọn bank transaction</option>{allBanks.map((item) => <option key={item.id} value={item.id}>{item.externalId} · {item.amount}</option>)}</select><button type="button" disabled={!payoutId || !bankId} className={button} onClick={() => createCase.mutate()}>Tạo case</button></div>{allCases.map((item) => <div className="mt-3 flex justify-between text-sm" key={item.id}><span>{item.status} · chênh {item.difference}</span>{item.status === "Candidate" && <button type="button" className={button} onClick={() => confirm.mutate(item.id)}>Checker xác nhận</button>}</div>)}</section>
    <section className="rounded-lg border border-slate-200 bg-white p-5"><h2 className="font-semibold">3. Ngoại lệ</h2>{allExceptions.length === 0 && <p className="mt-3 text-sm text-slate-500">Chưa có ngoại lệ.</p>}{allExceptions.map((item) => <div className="mt-3 flex justify-between gap-2 text-sm" key={item.id}><span>{item.reasonCode} · {item.status}</span><button type="button" className={button} onClick={() => actException.mutate({ id: item.id, action: item.status ?? "Open" })}>{item.status === "Open" ? "Yêu cầu đóng" : item.status === "PendingApproval" ? "Checker duyệt" : "Mở lại"}</button></div>)}</section></div>
    <div className="grid gap-5 xl:grid-cols-2"><section className="rounded-lg border border-slate-200 bg-white p-5"><h2 className="font-semibold">4. Ledger</h2><button type="button" className={`${button} mt-3`} onClick={() => postJournal.mutate()}>Post journal cân Nợ/Có</button>{allJournals.map((item) => <p className="mt-2 text-sm" key={item.id}>{item.status} · {item.period}</p>)}</section><section className="rounded-lg border border-slate-200 bg-white p-5"><h2 className="font-semibold">5. Classification</h2><button type="button" className={`${button} mt-3`} onClick={() => createMapping.mutate()}>Thêm mapping Revenue</button><p className="mt-2 text-sm text-slate-500">{rows(mappings.data).length} mapping</p><select className={`${input} mt-3 w-full`} value={batchId} onChange={(event) => setBatchId(event.target.value)}><option value="">Chọn batch</option>{allImports.map((item) => <option key={item.id} value={item.id}>{item.shopCode} · {item.period}</option>)}</select><button type="button" disabled={!batchId} className={`${button} mt-2`} onClick={() => classify.mutate()}>Chạy classification</button></section></div>
  </div>;
}

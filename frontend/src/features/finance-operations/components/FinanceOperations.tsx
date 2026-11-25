"use client";

import { createContext, type FormEvent, type ReactNode, useContext, useState } from "react";
import { useSearchParams } from "next/navigation";
import { useMutation, useQuery } from "@tanstack/react-query";
import { apiError } from "../../../services/http";
import { importService } from "../../../services/finance/import.service";
import { reconciliationService } from "../../../services/finance/reconciliation.service";
import { exceptionService } from "../../../services/finance/exception.service";
import { ledgerService } from "../../../services/finance/ledger.service";
import { classificationService } from "../../../services/finance/classification.service";
import type { ExceptionAction, FinanceException, ImportBatchInput, ImportSourceType } from "../../../services/finance/types";
import { useFinanceOperations } from "../hooks/useFinanceOperations";
import { parseStatementCsv } from "../utils/statementCsv";
import { LedgerReportPanel } from "./LedgerReportPanel";
import { ExtensionPanel } from "./ExtensionPanel";
import { extensionService } from "../../../services/finance/extension.service";
import { ControlsPanel } from "./ControlsPanel";
import { DataDetail } from "../../../components/ui/DataDetail";
import { MetricCard } from "../../../components/ui/MetricCard";

const control = "finance-control";
const button = "finance-button";
const sections = [
  { id: "imports", label: "Nhập dữ liệu", description: "Kiểm tra nguồn, xem trước và xác nhận batch trước khi phân loại." },
  { id: "reconciliation", label: "Đối soát", description: "Liên kết đơn, statement và tiền ngân hàng với bằng chứng nguồn." },
  { id: "exceptions", label: "Ngoại lệ", description: "Theo dõi chênh lệch, phản hồi bên ngoài và các quyết định xử lý." },
  { id: "ledger", label: "Sổ quản trị", description: "Ghi nhận bút toán cân bằng, tạo reversal và kiểm soát kỳ." },
  { id: "classification", label: "Phân loại", description: "Quản lý mapping có hiệu lực và xử lý các dòng chưa phân loại." },
  { id: "report", label: "Báo cáo nguồn", description: "Đọc số liệu từ journal và truy về bút toán, chứng từ liên quan." },
  { id: "extensions", label: "Đơn & chứng từ", description: "Quản lý nguồn đơn, hoàn tiền, giá vốn và hồ sơ chứng từ." },
  { id: "controls", label: "Kiểm soát", description: "Quản lý dung sai, phê duyệt mở kỳ và chuyển nguồn có thẩm quyền." },
] as const;
type SectionId = typeof sections[number]["id"];
const WorkspaceContext = createContext<SectionId>("imports");
const money = (value: number) => new Intl.NumberFormat("vi-VN", { maximumFractionDigits: 2 }).format(value);
const text = (form: FormData, key: string) => String(form.get(key) ?? "").trim();
const number = (form: FormData, key: string) => Number(text(form, key));
const month = (value: string) => `${value}-01`;

function Field({ label, name, type = "text", required = true, value, min, step }: { label: string; name: string; type?: string; required?: boolean; value?: string; min?: number; step?: string }) {
  return <label className="block text-sm text-slate-600"><span className="mb-1 block">{label}</span><input className={control} name={name} type={type} required={required} defaultValue={value} min={min} step={step} /></label>;
}
function Card({ title, children, section }: { title: string; children: ReactNode; section?: SectionId }) {
  const current = useContext(WorkspaceContext);
  return <section hidden={section ? current !== section : undefined} className="finance-panel" id={section ? `workspace-${section}` : undefined} role={section ? "tabpanel" : undefined} tabIndex={section ? 0 : undefined} aria-labelledby={section ? `tab-${section}` : undefined}><h2 className="mb-5 text-base font-semibold text-slate-900">{title}</h2>{children}</section>;
}
function JsonDetail({ children }: { children: unknown }) {
  return <div className="mt-3"><DataDetail value={children} /></div>;
}

export function FinanceOperations() {
  const searchParams = useSearchParams();
  const activeSection = sections.find(section => section.id === searchParams?.get("section")) ?? sections[0];
  const data = useFinanceOperations();
  const [shop, setShop] = useState("");
  const [currency, setCurrency] = useState("VND");
  const [period, setPeriod] = useState("");
  const [csv, setCsv] = useState("");
  const [previewPayload, setPreviewPayload] = useState<ImportBatchInput>();
  const [message, setMessage] = useState("");
  const [localError, setLocalError] = useState("");
  const [journalLineCount, setJournalLineCount] = useState(2);
  const preview = useMutation({ mutationFn: importService.preview, onSuccess: () => setMessage("Đã kiểm tra dữ liệu. Chưa ghi vào hệ thống; chọn Xác nhận nhập để commit.") });
  const session = data.session.data;
  const shopCode = shop || session?.shops[0] || "";
  const orders = useQuery({ queryKey: ["finance", "orders", shopCode], queryFn: () => extensionService.orders(shopCode), enabled: !!session && !!shopCode });
  const actor = session?.username ?? "";
  const roles = session?.roles ?? [];
  const permitted = (...allowed: string[]) => allowed.some((role) => roles.includes(role)) || (allowed.includes("Maker") && roles.includes("Admin"));
  const pending = data.action.isPending;
  const submit = (event: FormEvent<HTMLFormElement>, run: (form: FormData) => Promise<unknown>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    setLocalError(""); setMessage("");
    data.action.mutate(() => run(form), { onSuccess: () => setMessage("Đã lưu thay đổi."), onError: (error) => setLocalError(apiError(error)) });
  };
  const sourceScope = () => {
    if (!shopCode) throw new Error("Chọn shop trước khi thao tác.");
    return { shopCode, currency };
  };
  const queryErrors = [data.imports, data.payouts, data.banks, data.cases, data.exceptions, data.journals, data.mappings, data.classified].filter((query) => query.isError);
  const isLoading = [data.imports, data.payouts, data.banks, data.cases, data.exceptions, data.journals].some((query) => query.isLoading);
  const errors = localError || (preview.isError ? apiError(preview.error) : "");

  if (data.session.isLoading) return <div role="status" className="finance-panel space-y-4"><div className="h-5 w-44 animate-pulse rounded bg-slate-100" /><div className="h-10 max-w-lg animate-pulse rounded bg-slate-100" /><p className="text-sm text-slate-500">Đang kiểm tra phiên đăng nhập…</p></div>;
  if (!session) return <div className="mx-auto grid max-w-4xl gap-8 py-6 lg:grid-cols-[1fr_1.1fr] lg:py-12"><div className="self-center"><p className="mb-3 text-xs font-semibold uppercase tracking-widest text-slate-500">Không gian làm việc Finance</p><h2 className="text-3xl font-semibold leading-tight tracking-tight text-slate-900">Dữ liệu rõ ràng.<br />Quyết định có kiểm soát.</h2><p className="mt-4 max-w-sm text-sm leading-7 text-slate-600">Đăng nhập để truy cập nguồn dữ liệu và thực hiện nghiệp vụ theo quyền được giao.</p><ol className="mt-6 space-y-3 text-sm text-slate-600"><li>01 · Nhập và kiểm tra chứng từ nguồn</li><li>02 · Đối soát, xử lý chênh lệch</li><li>03 · Ghi sổ và phê duyệt độc lập</li></ol></div><Card title="Đăng nhập Finance"><form className="space-y-5" onSubmit={(event) => { event.preventDefault(); const form = new FormData(event.currentTarget); data.login.mutate({ username: text(form, "username"), password: text(form, "password") }); }}><Field label="Tên đăng nhập" name="username" /><Field label="Mật khẩu" name="password" type="password" /><button className={`${button} w-full`} disabled={data.login.isPending}>{data.login.isPending ? "Đang đăng nhập…" : "Đăng nhập"}</button>{data.login.isError && <p role="alert" className="finance-alert" data-tone="danger">{apiError(data.login.error)}</p>}<p className="border-t border-slate-100 pt-4 text-xs leading-6 text-slate-500">Danh tính và phạm vi cửa hàng được kiểm tra tại hệ thống. Người đề xuất và người duyệt thực hiện bằng hai tài khoản độc lập.</p></form></Card></div>;

  const allImports = data.imports.data ?? [];
  const allCases = data.cases.data ?? [];
  const allExceptions = data.exceptions.data ?? [];
  const allJournals = data.journals.data ?? [];
  const statementLines = allImports.filter((batch) => batch.shopCode === shopCode && batch.currency === currency && batch.sourceType === "MarketplaceStatement").flatMap((batch) => batch.lines).filter((line) => line.status === "Valid");
  const renderExceptionActions = (item: FinanceException) => {
    if (item.status === "PendingApproval" ? !permitted("Checker") : !permitted("Maker", "Controller")) return null;
    return <form className="mt-4 grid gap-3 sm:grid-cols-2" onSubmit={(event) => submit(event, (form) => {
    const action: ExceptionAction = { actor, note: text(form, "note"), evidence: text(form, "evidence"), outcome: text(form, "outcome") as ExceptionAction["outcome"] || undefined };
    const choice = text(form, "action");
    if (choice === "wait") return exceptionService.waitExternal(item.id, action);
    if (choice === "request") return exceptionService.requestClosure(item.id, action);
    if (choice === "approve") return exceptionService.approveClosure(item.id, action);
    return exceptionService.reopen(item.id, action);
  })}><Field label="Lý do / kết quả xử lý" name="note" /><Field label="Tham chiếu bằng chứng" name="evidence" /><label className="text-sm">Thao tác<select name="action" className={control} required>
    {(item.status === "Open" || item.status === "WaitingExternal") && permitted("Maker", "Controller") && <>{item.status === "Open" && <option value="wait">Chờ phản hồi bên ngoài</option>}<option value="request">Đề nghị đóng</option></>}
    {item.status === "PendingApproval" && permitted("Checker") && <option value="approve">Duyệt đóng</option>}
    {item.status === "Closed" && permitted("Maker", "Controller") && <option value="reopen">Mở lại với bằng chứng mới</option>}
  </select></label><label className="text-sm">Kết quả xác minh<select name="outcome" className={control}><option value="">Chưa xác minh</option><option value="Verified">Đã xác minh kết quả</option><option value="AcceptVariance">Đề nghị chấp nhận chênh lệch</option></select></label><button disabled={pending} className={button}>Gửi quyết định</button></form>;
  };

  return <WorkspaceContext.Provider value={activeSection.id}><div className="space-y-5 finance-operations">
    <Card title="Phiên làm việc"><div className="flex flex-wrap items-end gap-4"><p className="self-center text-sm"><strong>{actor}</strong> · {roles.join(", ")}</p><label className="text-sm">Shop<select aria-label="Shop làm việc" className={control} value={shopCode} onChange={(event) => { setShop(event.target.value); setPreviewPayload(undefined); preview.reset(); }}>{session.shops.map((value) => <option key={value}>{value}</option>)}</select></label><label className="text-sm">Đồng tiền<input className={control} value={currency} onChange={(event) => { setCurrency(event.target.value.toUpperCase()); setPreviewPayload(undefined); preview.reset(); }} maxLength={3} /></label><label className="text-sm">Kỳ báo cáo<input aria-label="Kỳ báo cáo" type="month" value={period} onChange={(event) => { setPeriod(event.target.value); setPreviewPayload(undefined); preview.reset(); }} className={control} /></label><button type="button" className={button} disabled={data.logout.isPending || pending} onClick={() => data.logout.mutate()}>Đăng xuất</button></div></Card>
    {errors && <p role="alert" className="rounded border border-red-200 bg-red-50 p-3 text-sm text-red-800">{errors}</p>}
    {message && <p role="status" className="rounded border border-blue-200 bg-blue-50 p-3 text-sm text-blue-900">{message}</p>}
    {queryErrors.length > 0 && <p role="alert" className="rounded bg-amber-50 p-3 text-sm text-amber-800">Không tải được một số dữ liệu: {apiError(queryErrors[0].error)} <button type="button" onClick={() => queryErrors.forEach((query) => void query.refetch())} className="underline">Thử lại</button></p>}
    <div className="grid grid-cols-2 gap-3 xl:grid-cols-4">{[["Lô dữ liệu nhập", allImports.length], ["Case chênh lệch", allCases.filter((item) => item.status === "Disputed").length], ["Ngoại lệ mở", allExceptions.filter((item) => item.status !== "Closed").length], ["Bút toán", allJournals.length]].map(([label, value]) => <MetricCard key={String(label)} label={String(label)} value={isLoading || queryErrors.length ? "—" : money(Number(value))} />)}</div>

    <div className="finance-workspace-nav"><div role="tablist" aria-label="Nghiệp vụ Finance" className="flex gap-1 overflow-x-auto border-b border-slate-200 pb-1">{sections.map(section => <button key={section.id} id={`tab-${section.id}`} type="button" role="tab" disabled={pending || preview.isPending} aria-controls={`workspace-${section.id}`} aria-selected={activeSection.id === section.id} tabIndex={activeSection.id === section.id ? 0 : -1} onClick={() => { setMessage(""); setLocalError(""); window.history.pushState(null, "", `/van-hanh-finance?section=${section.id}`); }} onKeyDown={event => { if (event.key !== "ArrowRight" && event.key !== "ArrowLeft" && event.key !== "Home" && event.key !== "End") return; event.preventDefault(); const index = sections.findIndex(item => item.id === section.id); const next = event.key === "Home" ? 0 : event.key === "End" ? sections.length - 1 : (index + (event.key === "ArrowRight" ? 1 : -1) + sections.length) % sections.length; const target = document.getElementById(`tab-${sections[next].id}`); target?.focus(); target?.click(); }} className={`whitespace-nowrap rounded-t-lg px-3 py-3 text-sm font-medium transition ${activeSection.id === section.id ? "bg-white text-[#17375e] shadow-[inset_0_-2px_0_#17375e]" : "text-slate-500 hover:bg-slate-100 hover:text-slate-900"}`}>{section.label}</button>)}</div><p className="mt-3 text-sm text-slate-500">{activeSection.description}</p></div>

    <Card section="imports" title="Nhập nguồn và lịch sử batch"><p className="mb-3 text-sm text-slate-500">CSV UTF-8 gồm 4 cột: externalId, amount, occurredOn (yyyy-MM-dd), businessType. Chọn kỳ và shop trước khi kiểm tra.</p>{permitted("Maker") && <form className="space-y-3" onSubmit={(event) => { event.preventDefault(); try { if (!period) throw new Error("Chọn kỳ báo cáo trước khi nhập."); const form = new FormData(event.currentTarget); const payload: ImportBatchInput = { ...sourceScope(), period: month(period), importedBy: actor, sourceType: text(form, "sourceType") as ImportSourceType, lines: parseStatementCsv(csv) }; setLocalError(""); setPreviewPayload(payload); preview.mutate(payload); } catch (error) { setLocalError(apiError(error)); } }}><label className="block text-sm">Loại nguồn<select name="sourceType" className={control} onChange={() => { preview.reset(); setPreviewPayload(undefined); }}><option value="MarketplaceStatement">Statement sàn</option><option value="BankStatement">Sao kê ngân hàng</option></select></label><label className="block text-sm">Chọn CSV<input type="file" accept=".csv,text/csv" className="mt-1 block text-sm" onChange={async (event) => { const file = event.target.files?.[0]; preview.reset(); setPreviewPayload(undefined); if (!file) return; if (file.size > 5 * 1024 * 1024) { setLocalError("File tối đa 5 MB."); return; } setCsv(await file.text()); }} /></label>
      <div><label htmlFor="statement-csv" className="block text-sm">Nội dung CSV</label><textarea id="statement-csv" name="csv" value={csv} onChange={(event) => { setCsv(event.target.value); preview.reset(); setPreviewPayload(undefined); }} required className={`${control} mt-1 min-h-28 font-mono`} /></div>
      <button className={button} disabled={preview.isPending || pending}>Kiểm tra trước khi nhập</button></form>}
      {preview.data && <div className="mt-4 rounded border border-blue-100 p-3"><p className="text-sm">Hợp lệ: {preview.data.validLines} · Quarantine: {preview.data.quarantinedLines} {preview.data.isDuplicate ? "· Batch đã tồn tại" : ""}</p><JsonDetail>{preview.data.lines}</JsonDetail><button type="button" disabled={pending || !previewPayload} className={`${button} mt-3`} onClick={() => { if (previewPayload) data.action.mutate(() => importService.create(previewPayload), { onSuccess: () => { setMessage("Đã commit batch."); preview.reset(); setPreviewPayload(undefined); }, onError: (error) => setLocalError(apiError(error)) }); }}>Xác nhận nhập</button></div>}
      <div className="mt-4 space-y-2">{!allImports.length && <p className="text-sm text-slate-500">Chưa có batch.</p>}{allImports.map((batch) => <details key={batch.id} className="rounded border p-3 text-sm"><summary className="cursor-pointer">{batch.shopCode} · {batch.period} · {batch.sourceType} · {batch.lines.length} dòng</summary><p className="mt-2 break-all text-xs">Batch: {batch.id} · Checksum: {batch.checksum}</p><JsonDetail>{batch.lines}</JsonDetail></details>)}</div>
    </Card>

    <Card section="reconciliation" title="Payout, giao dịch ngân hàng và đối soát">{permitted("Maker") && <div className="grid gap-5 xl:grid-cols-2"><form className="grid gap-3 sm:grid-cols-2" onSubmit={(event) => submit(event, (form) => reconciliationService.createPayout({ ...sourceScope(), externalId: text(form, "externalId"), amount: number(form, "amount"), payoutOn: text(form, "date") }))}><h3 className="font-medium sm:col-span-2">Thêm payout</h3><Field label="Mã payout nguồn" name="externalId" /><Field label="Số tiền payout" name="amount" type="number" min={0.01} step="0.01" /><Field label="Ngày payout" name="date" type="date" /><button className={`${button} self-end`} disabled={pending}>Lưu payout</button></form><form className="grid gap-3 sm:grid-cols-2" onSubmit={(event) => submit(event, (form) => reconciliationService.createBankTransaction({ ...sourceScope(), externalId: text(form, "externalId"), amount: number(form, "amount"), creditedOn: text(form, "date") }))}><h3 className="font-medium sm:col-span-2">Thêm giao dịch ngân hàng</h3><Field label="Mã giao dịch ngân hàng" name="externalId" /><Field label="Số tiền ghi có" name="amount" type="number" min={0.01} step="0.01" /><Field label="Ngày ngân hàng ghi có" name="date" type="date" /><button className={`${button} self-end`} disabled={pending}>Lưu bank transaction</button></form></div>}
      {permitted("Maker") && <form className="mt-5 grid gap-3 rounded border bg-slate-50 p-4 sm:grid-cols-2" onSubmit={(event) => submit(event, (form) => reconciliationService.createCase({ payoutId: text(form, "payoutId"), bankTransactionId: text(form, "bankTransactionId"), tolerance: 0, evidence: text(form, "evidence"), proposedBy: actor, reason: text(form, "reason"), allocations: form.getAll("statementLine").map((id) => { const line = statementLines.find((item) => item.id === String(id)); if (!line) throw new Error("Không tìm thấy dòng statement."); return { importLineId: line.id, amount: line.amount, orderLineId: text(form, `order-${line.id}`) || undefined }; }) }))}>
        <label className="text-sm">Payout<select aria-label="Payout" className={control} name="payoutId" required><option value="">Chọn payout</option>{data.payouts.data?.filter((item) => item.shopCode === shopCode && item.currency === currency).map((item) => <option key={item.id} value={item.id}>{item.externalId} · {money(item.amount)} {item.currency}</option>)}</select></label>
        <label className="text-sm">Giao dịch ngân hàng<select aria-label="Giao dịch ngân hàng" className={control} name="bankTransactionId" required><option value="">Chọn bank transaction</option>{data.banks.data?.filter((item) => item.shopCode === shopCode && item.currency === currency).map((item) => <option key={item.id} value={item.id}>{item.externalId} · {money(item.amount)} {item.currency}</option>)}</select></label>
        <Field label="Bằng chứng liên kết payout và bank" name="evidence" /><Field label="Lý do ghép chứng từ" name="reason" />
        <fieldset className="rounded border p-3 sm:col-span-2"><legend className="px-1 text-sm font-medium">Dòng statement phân bổ vào payout</legend>{!statementLines.length && <p className="text-sm text-slate-500">Chưa có dòng hợp lệ. Nhập statement trước khi đối soát hoàn tất.</p>}{statementLines.map((line) => <div className="my-2 grid gap-2 sm:grid-cols-2" key={line.id}><label className="flex items-center gap-2 text-sm"><input type="checkbox" name="statementLine" value={line.id} />{line.externalId} · {line.businessType} · {money(line.amount)}</label>{line.amount > 0 && <label className="text-sm">Dòng đơn cho {line.externalId}<select aria-label={`Dòng đơn cho ${line.externalId}`} name={`order-${line.id}`} className={control}><option value="">Chọn nguồn đơn</option>{orders.data?.filter((order) => order.currency === currency).map((order) => <option key={order.id} value={order.id}>{order.orderId}/{order.externalId} · {order.sku}</option>)}</select></label>}</div>)}</fieldset>
        <button className={`${button} self-end`} disabled={pending}>Đề xuất đối soát</button><p className="text-xs text-slate-500 sm:col-span-2">Dung sai do chính sách phía hệ thống xác định. Tổng dòng statement phải bằng payout và khoản doanh thu cần liên kết dòng đơn để xác nhận.</p>
      </form>}
      <div className="mt-4 space-y-2">{!allCases.length && <p className="text-sm text-slate-500">Chưa có case đối soát.</p>}{allCases.map((item) => <details key={item.id} className="rounded border p-3 text-sm"><summary className="cursor-pointer">{item.status} · Chênh: {money(item.difference)} · Người đề xuất: {item.proposedBy}</summary><JsonDetail>{item}</JsonDetail>{item.status === "Candidate" && permitted("Checker") && <button type="button" disabled={pending || item.proposedBy === actor} className={`${button} mt-2`} onClick={() => data.action.mutate(() => reconciliationService.confirm(item.id, actor), { onError: (error) => setLocalError(apiError(error)) })}>Xác nhận đối soát</button>}</details>)}</div>
    </Card>

    <Card section="exceptions" title="Ngoại lệ, bằng chứng và phê duyệt">{permitted("Maker") && <details className="mb-4 rounded border p-3 text-sm"><summary className="cursor-pointer">Tạo hồ sơ ngoại lệ</summary><form className="mt-3 grid gap-3 sm:grid-cols-2" onSubmit={(event) => submit(event, (form) => exceptionService.create({ referenceType: text(form, "referenceType"), referenceId: text(form, "referenceId"), shopCode, reasonCode: text(form, "reasonCode"), amount: text(form, "amount") ? number(form, "amount") : undefined, owner: text(form, "owner"), dueOn: text(form, "dueOn"), evidence: text(form, "evidence"), createdBy: actor }))}><Field label="Loại tham chiếu" name="referenceType" /><Field label="Mã tham chiếu" name="referenceId" /><Field label="Mã nguyên nhân" name="reasonCode" /><Field label="Số tiền ngoại lệ" name="amount" type="number" step="0.01" required={false} /><Field label="Người phụ trách" name="owner" /><Field label="Hạn xử lý" name="dueOn" type="date" /><Field label="Bằng chứng nguồn" name="evidence" /><button className={`${button} self-end`} disabled={pending}>Tạo ngoại lệ</button></form></details>}
      <div className="space-y-3">{!allExceptions.length && <p className="text-sm text-slate-500">Chưa có ngoại lệ.</p>}{allExceptions.map((item) => <details key={item.id} className="rounded border p-3 text-sm"><summary className="cursor-pointer">{item.reasonCode} · {item.status} · {item.owner} · Hạn {item.dueOn}</summary><p className="mt-2">Nguồn: {item.referenceType}/{item.referenceId} · {item.evidence}</p><ol className="mt-3 space-y-2 border-l-2 border-blue-100 pl-3">{item.activities.map((activity) => <li key={activity.id}><strong>{activity.action}</strong> · {activity.actor} · {new Date(activity.occurredAt).toLocaleString("vi-VN")}<p>{activity.note}</p><p className="text-xs text-slate-500">Evidence: {activity.evidence}</p></li>)}</ol>{renderExceptionActions(item)}</details>)}</div>
    </Card>

    <Card section="ledger" title="Sổ quản trị và khóa kỳ">{permitted("Maker", "Controller") && <form className="grid gap-3 sm:grid-cols-2" onSubmit={(event) => submit(event, (form) => { if (!period) return Promise.reject(new Error("Chọn kỳ báo cáo trước khi post.")); const lines = Array.from({ length: journalLineCount }, (_, index) => ({ accountCode: text(form, `account-${index}`), debit: number(form, `debit-${index}`), credit: number(form, `credit-${index}`) })); const debit = lines.reduce((sum, line) => sum + line.debit, 0), credit = lines.reduce((sum, line) => sum + line.credit, 0); if (Math.abs(debit - credit) > 0.000001) return Promise.reject(new Error("Tổng Nợ phải bằng tổng Có.")); return ledgerService.post({ ...sourceScope(), referenceType: text(form, "referenceType"), referenceId: text(form, "referenceId"), period: month(period), postedBy: actor, lines, reason: text(form, "reason"), evidence: text(form, "evidence") }); })}>
      <Field label="Loại sự kiện nguồn" name="referenceType" /><Field label="ID sự kiện nguồn" name="referenceId" /><Field label="Lý do bút toán" name="reason" /><Field label="Chứng từ bút toán" name="evidence" />
      <div className="space-y-3 sm:col-span-2">{Array.from({ length: journalLineCount }, (_, index) => <div key={index} className="grid gap-2 rounded bg-slate-50 p-3 sm:grid-cols-3"><Field label={`Tài khoản dòng ${index + 1}`} name={`account-${index}`} /><Field label={`Nợ dòng ${index + 1}`} name={`debit-${index}`} type="number" min={0} step="0.01" value="0" /><Field label={`Có dòng ${index + 1}`} name={`credit-${index}`} type="number" min={0} step="0.01" value="0" /></div>)}</div><div className="flex gap-2"><button type="button" className={button} disabled={pending || journalLineCount >= 30} onClick={() => setJournalLineCount((count) => count + 1)}>Thêm dòng</button><button type="button" className={button} disabled={pending || journalLineCount <= 2} onClick={() => setJournalLineCount((count) => count - 1)}>Bớt dòng</button></div><button disabled={pending} className={button}>Post journal</button></form>}
      {permitted("Controller") && <form className="mt-5 flex flex-wrap items-end gap-3 rounded border border-amber-200 p-3" onSubmit={(event) => submit(event, (form) => { if (text(form, "confirmation") !== "KHOA KY") return Promise.reject(new Error("Nhập KHOA KY để xác nhận.")); return ledgerService.closePeriod({ shopCode, period: month(text(form, "period")), closedBy: actor }); })}><Field label="Tháng cần khóa" name="period" type="month" /><Field label="Xác nhận bằng KHOA KY" name="confirmation" /><button className={button} disabled={pending}>Khóa kỳ</button></form>}
      <div className="mt-4 space-y-2">{!allJournals.length && <p className="text-sm text-slate-500">Chưa có journal.</p>}{allJournals.map((item) => <details key={item.id} className="rounded border p-3 text-sm"><summary className="cursor-pointer">{item.referenceId} · {item.status} · {item.period} · Nợ {money(item.lines.reduce((sum, line) => sum + line.debit, 0))}</summary><JsonDetail>{item}</JsonDetail>{item.status === "Posted" && permitted("Controller") && <form className="mt-3 flex flex-wrap items-end gap-3" onSubmit={(event) => submit(event, (form) => ledgerService.reverse(item.id, { period: month(text(form, "period")), postedBy: actor }))}><Field label="Kỳ tạo bút toán đảo" name="period" type="month" /><button disabled={pending} className={button}>Tạo reversal</button></form>}</details>)}</div>
    </Card>

    <Card section="classification" title="Mapping và phân loại">{permitted("Admin") && <form className="grid gap-3 sm:grid-cols-3" onSubmit={(event) => submit(event, (form) => classificationService.addMapping({ sourceType: text(form, "sourceType") as ImportSourceType, sourceBusinessType: text(form, "sourceBusinessType"), targetCategory: text(form, "targetCategory"), version: number(form, "version"), effectiveFrom: text(form, "effectiveFrom"), effectiveTo: text(form, "effectiveTo") || undefined, createdBy: actor }))}><label className="text-sm">Nguồn mapping<select name="sourceType" className={control}><option value="MarketplaceStatement">Statement sàn</option><option value="BankStatement">Ngân hàng</option></select></label><Field label="Loại nghiệp vụ nguồn" name="sourceBusinessType" /><Field label="Nhóm phân loại đích" name="targetCategory" /><Field label="Phiên bản" name="version" type="number" min={1} step="1" /><Field label="Hiệu lực từ" name="effectiveFrom" type="date" /><Field label="Hiệu lực đến" name="effectiveTo" type="date" required={false} /><button className={button} disabled={pending}>Lưu mapping</button></form>}
      <details className="mt-4 text-sm"><summary className="cursor-pointer">Danh sách mapping ({data.mappings.data?.length ?? 0})</summary><JsonDetail>{data.mappings.data}</JsonDetail></details>
      {permitted("Maker") && <form className="mt-4 grid gap-3 sm:grid-cols-2" onSubmit={(event) => submit(event, async (form) => { const result = await classificationService.classifyBatch(text(form, "batchId"), { effectiveOn: text(form, "effectiveOn"), actor }); return result; })}><label className="text-sm">Batch cần phân loại<select name="batchId" className={control} required><option value="">Chọn batch</option>{allImports.filter((batch) => batch.shopCode === shopCode).map((batch) => <option key={batch.id} value={batch.id}>{batch.period} · {batch.id}</option>)}</select></label><Field label="Ngày đối chiếu hiệu lực mapping" name="effectiveOn" type="date" /><button className={button} disabled={pending}>Phân loại / xử lý lại Unmapped</button></form>}
      <details className="mt-4 text-sm"><summary className="cursor-pointer">Kết quả phân loại ({data.classified.data?.length ?? 0})</summary><JsonDetail>{data.classified.data}</JsonDetail></details>
    </Card>
    <Card section="report" title="Tổng hợp và truy nguồn journal"><LedgerReportPanel shopCode={shopCode} currency={currency} period={period} /></Card>
    <Card section="extensions" title="Đơn nguồn, hoàn tiền, giá vốn và chứng từ"><ExtensionPanel shopCode={shopCode} currency={currency} actor={actor} roles={roles} /></Card>
    <Card section="controls" title="Chính sách, mở lại kỳ và chuyển nguồn"><ControlsPanel shopCode={shopCode} currency={currency} roles={roles} username={actor} period={period} /></Card>
  </div></WorkspaceContext.Provider>;
}

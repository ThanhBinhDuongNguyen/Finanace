"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { type FormEvent } from "react";
import { controlsService } from "../../../services/finance/controls.service";
import { apiError } from "../../../services/http";
import { DataDetail } from "../../../components/ui/DataDetail";

const input = "finance-control";
const button = "finance-button";
const value = (form: FormData, name: string) => String(form.get(name) ?? "").trim();

function manifest(value: string): unknown { try { return JSON.parse(value); } catch { return value; } }

export type ControlsPanelProps = { shopCode: string; currency: string; roles: string[]; username: string; period: string };

export function ControlsPanel({ shopCode, currency, roles, username, period }: ControlsPanelProps) {
  const cache = useQueryClient();
  const policies = useQuery({ queryKey: ["finance", "controls", "policies"], queryFn: controlsService.policies });
  const reopens = useQuery({ queryKey: ["finance", "controls", "reopen"], queryFn: controlsService.reopenRequests });
  const sources = useQuery({ queryKey: ["finance", "controls", "sources"], queryFn: controlsService.sources });
  const action = useMutation({ mutationFn: (run: () => Promise<unknown>) => run(), onSuccess: () => cache.invalidateQueries({ queryKey: ["finance"] }) });
  const current = sources.data?.authorities.find((source) => source.shopCode === shopCode);
  const currentMode = current?.mode ?? "Local";
  const currentVersion = current?.version ?? 1;
  const canAdmin = roles.includes("Admin"), canController = roles.includes("Controller"), canChecker = roles.includes("Checker");
  const busy = action.isPending;
  const month = period.slice(0, 7);
  const submit = (event: FormEvent<HTMLFormElement>, run: (form: FormData) => Promise<unknown>) => { event.preventDefault(); const form = new FormData(event.currentTarget); action.mutate(() => run(form)); };
  const loadError = [policies, reopens, sources].find((query) => query.isError);
  return <section className="space-y-4 rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
    <h2 className="text-base font-semibold text-[#17375e]">Kiểm soát kỳ, dung sai và chuyển nguồn</h2>
    {action.isError && <p role="alert" className="text-sm text-red-700">{apiError(action.error)}</p>}
    {action.isSuccess && <p role="status" className="text-sm text-blue-700">Đã lưu quyết định kiểm soát.</p>}
    {loadError && <p role="alert" className="text-sm text-amber-700">{apiError(loadError.error)}</p>}
    {[policies, reopens, sources].some((query) => query.isLoading) && <p className="text-sm text-slate-500">Đang tải chính sách kiểm soát…</p>}
    <details className="rounded border p-3"><summary className="cursor-pointer text-sm font-medium">Chính sách dung sai đối soát</summary>
      <p className="mt-2 text-sm text-slate-600">Dung sai do server chọn theo currency và ngày payout. Khi chưa có policy hiệu lực, dung sai là 0.</p>
      {canAdmin && <form className="mt-3 grid gap-3 sm:grid-cols-2 lg:grid-cols-3" onSubmit={(event) => submit(event, (form) => controlsService.addPolicy({ currency: value(form, "currency"), amount: Number(value(form, "amount")), effectiveFrom: value(form, "from"), effectiveTo: value(form, "to"), evidence: value(form, "evidence") }))}>
        <label className="text-sm">Currency policy<input className={input} name="currency" defaultValue={currency} pattern="[A-Za-z]{3}" required /></label>
        <label className="text-sm">Dung sai tối đa<input className={input} name="amount" type="number" min="0" step="0.01" required /></label>
        <label className="text-sm">Hiệu lực từ<input className={input} name="from" type="date" required /></label>
        <label className="text-sm">Hiệu lực đến<input className={input} name="to" type="date" required /></label>
        <label className="text-sm">Bằng chứng phê duyệt policy<input className={input} name="evidence" required /></label>
        <button className={`${button} self-end`} disabled={busy}>Lưu policy dung sai</button>
      </form>}
      <div className="mt-3 space-y-2 text-sm">{policies.data?.length === 0 && <p className="text-slate-500">Chưa có policy dung sai.</p>}{policies.data?.map((policy) => <div key={policy.id} className="rounded bg-slate-50 p-2"><p>{policy.currency} · tối đa {new Intl.NumberFormat("vi-VN").format(policy.amount)} · {policy.effectiveFrom} → {policy.effectiveTo}</p><p className="text-xs text-slate-500">Phê duyệt: {policy.approvedBy} · Evidence: {policy.evidence}</p></div>)}</div>
    </details>
    <details className="rounded border p-3"><summary className="cursor-pointer text-sm font-medium">Yêu cầu mở lại kỳ đã khóa</summary>
      <p className="mt-2 text-sm text-slate-600">Admin lập yêu cầu; Controller khác người lập phê duyệt. Mọi yêu cầu giữ lý do và bằng chứng.</p>
      {canAdmin && <form className="mt-3 grid gap-3 sm:grid-cols-2" onSubmit={(event) => submit(event, (form) => controlsService.requestReopen({ shopCode, period: `${value(form, "period")}-01`, reason: value(form, "reason"), evidence: value(form, "evidence") }))}>
        <label className="text-sm">Tháng đề nghị mở lại<input className={input} name="period" type="month" defaultValue={month} required /></label>
        <label className="text-sm">Lý do mở kỳ<input className={input} name="reason" required /></label>
        <label className="text-sm">Bằng chứng mở kỳ<input className={input} name="evidence" required /></label>
        <button className={`${button} self-end`} disabled={busy || !shopCode}>Gửi yêu cầu mở kỳ</button>
      </form>}
      <div className="mt-3 space-y-2">{reopens.data?.filter((request) => request.shopCode === shopCode).map((request) => <details className="rounded bg-slate-50 p-2 text-sm" key={request.id}><summary className="cursor-pointer">{request.period} · {request.approvedBy ? `Đã duyệt bởi ${request.approvedBy}` : "Chờ Controller"}</summary><p className="mt-2">Người lập: {request.proposedBy}. Lý do: {request.reason}. Evidence: {request.evidence}</p>{canController && !request.approvedBy && request.proposedBy !== username && <form className="mt-3 space-y-2" onSubmit={(event) => submit(event, () => controlsService.approveReopen(request.id))}><label className="flex items-center gap-2"><input type="checkbox" required />Tôi xác nhận cho phép ghi lại vào kỳ {request.period}.</label><button className={button} disabled={busy}>Controller duyệt mở kỳ</button></form>}</details>)}</div>
    </details>
    <details className="rounded border p-3"><summary className="cursor-pointer text-sm font-medium">Nguồn có thẩm quyền của shop</summary>
      <p className="mt-2 text-sm">{shopCode}: nguồn {currentMode}, phiên bản {currentVersion}.</p>
      <p className="mt-1 text-sm text-slate-600">Chuyển sang Connected sẽ khóa writer Local. Rollback về Local sau chuyển nguồn cần quy trình riêng. Mapping và danh sách công việc đang mở được lưu trong hồ sơ chuyển giao.</p>
      {canAdmin && <form className="mt-3 grid gap-3 sm:grid-cols-2" onSubmit={(event) => submit(event, async (form) => {
        let mapping: unknown;
        try { mapping = JSON.parse(value(form, "mapping")); } catch { throw new Error("Mapping ID cần JSON object hợp lệ, ví dụ {\"local-id\":\"connected-id\"}."); }
        if (!mapping || Array.isArray(mapping) || typeof mapping !== "object" || Object.keys(mapping).length === 0 || Object.values(mapping).some((entry) => typeof entry !== "string" || !entry.trim())) throw new Error("Mapping phải là object có ít nhất một cặp ID chuỗi không rỗng.");
        return controlsService.requestSwitch({ shopCode, targetMode: value(form, "target") as "Local" | "Connected", expectedVersion: currentVersion, checkpoint: value(form, "checkpoint"), mapping: mapping as Record<string, string> });
      })}>
        <label className="text-sm">Nguồn đích<select name="target" className={input}><option value="Connected">Connected</option><option value="Local">Local</option></select></label>
        <label className="text-sm">Checkpoint chuyển nguồn<input className={input} name="checkpoint" required /></label>
        <label className="text-sm sm:col-span-2">Mapping ID (JSON)<textarea className={`${input} min-h-24 font-mono`} name="mapping" placeholder={'{"local-id":"connected-id"}'} required /></label>
        <button className={`${button} self-end`} disabled={busy || !shopCode || !sources.isSuccess}>Lập yêu cầu chuyển nguồn</button>
      </form>}
      <div className="mt-3 space-y-2">{sources.data?.requests.filter((request) => request.shopCode === shopCode).map((request) => <details key={request.id} className="rounded bg-slate-50 p-2 text-sm"><summary className="cursor-pointer">{request.targetMode} · checkpoint {request.checkpoint} · {request.approvedBy ? `Đã duyệt bởi ${request.approvedBy}` : "Chờ Checker"}</summary><p className="mt-2">Người lập: {request.proposedBy} · phiên bản nguồn: {request.expectedVersion}</p><p className="mt-2 text-xs text-slate-500">Mapping ID</p><DataDetail value={manifest(request.mappingManifest)} /><p className="mt-2 text-xs text-slate-500">Công việc mở tại thời điểm lập</p><DataDetail value={manifest(request.openWorkManifest)} emptyMessage="Không có hồ sơ mở tại checkpoint này." />{canChecker && !request.approvedBy && request.proposedBy !== username && <form className="mt-3 space-y-2" onSubmit={(event) => submit(event, () => controlsService.approveSwitch(request.id))}><label className="flex items-center gap-2"><input type="checkbox" required />Tôi xác nhận checkpoint, mapping và công việc mở trước khi khóa writer cũ.</label><button className={button} disabled={busy}>Checker duyệt chuyển nguồn</button></form>}</details>)}</div>
    </details>
  </section>;
}

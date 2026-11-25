"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { extensionService } from "../../../services/finance/extension.service";
import { apiError } from "../../../services/http";

const input = "finance-control";
const button = "finance-button";

export function PayoutAccountPanel({ shopCode, actor, roles }: { shopCode: string; actor: string; roles: string[] }) {
  const cache = useQueryClient();
  const data = useQuery({ queryKey: ["finance", "payout-accounts", shopCode], queryFn: () => extensionService.payoutAccounts(shopCode) });
  const action = useMutation({ mutationFn: (run: () => Promise<unknown>) => run(), onSuccess: () => cache.invalidateQueries({ queryKey: ["finance"] }) });
  return <details className="rounded border border-slate-200 p-3">
    <summary className="cursor-pointer text-sm font-medium">Tài khoản nhận tiền và lịch sử thay đổi</summary>
    <p className="mt-2 text-sm text-slate-600">Chỉ cập nhật tài khoản sau khi người khác duyệt bằng chứng xác minh kênh. Chưa có xác minh tự động từ ngân hàng.</p>
    {data.isLoading && <p role="status">Đang tải tài khoản…</p>}
    {(data.isError || action.isError) && <p role="alert" className="mt-2 text-sm text-red-700">{apiError(data.error ?? action.error)}</p>}
    {action.isSuccess && <p role="status" className="mt-2 text-sm text-blue-700">Đã lưu hồ sơ tài khoản.</p>}
    {data.data?.history.map(account => <p key={account.id} className="mt-2 text-sm">{account.accountReference} · {account.deactivatedAt ? "Đã ngừng sử dụng" : "Đang sử dụng"}</p>)}
    {(roles.includes("Maker") || roles.includes("Admin")) && <form className="mt-3 grid gap-3 sm:grid-cols-2" onSubmit={event => {
      event.preventDefault(); const form = new FormData(event.currentTarget);
      const text = (name: string) => String(form.get(name) ?? "").trim();
      action.mutate(() => extensionService.proposePayoutAccount({ shopCode, sourceRequestId: text("sourceRequestId"), newAccountReference: text("account"), independentChannelEvidence: text("evidence"), reason: text("reason") }));
    }}>
      <label className="text-sm">ID yêu cầu thay tài khoản<input name="sourceRequestId" className={input} required /></label>
      <label className="text-sm">Tham chiếu tài khoản nhận mới<input name="account" className={input} required /></label>
      <label className="text-sm">Bằng chứng xác minh kênh độc lập<input name="evidence" className={input} required /></label>
      <label className="text-sm">Lý do đổi tài khoản<input name="reason" className={input} required /></label>
      <button className={button} disabled={action.isPending}>Đề xuất thay tài khoản</button>
    </form>}
    {data.data?.changes.map(change => <details key={change.id} className="mt-3 rounded bg-slate-50 p-3 text-sm">
      <summary>{change.sourceRequestId} · {change.status} · {change.newAccountReference}</summary>
      <p className="mt-2">Người đề xuất: {change.proposedBy}</p><p>Bằng chứng: {change.independentChannelEvidence}</p>
      {roles.includes("Checker") && change.proposedBy !== actor && change.status === "PendingApproval" && <form className="mt-3 flex gap-2" onSubmit={event => { event.preventDefault(); const evidence = String(new FormData(event.currentTarget).get("evidence") ?? ""); action.mutate(() => extensionService.approvePayoutAccount(change.id, evidence)); }}>
        <label className="flex-1">Bằng chứng duyệt tài khoản<input className={input} name="evidence" required /></label>
        <button className={`${button} self-end`} disabled={action.isPending}>Duyệt tài khoản nhận</button>
      </form>}
    </details>)}
  </details>;
}

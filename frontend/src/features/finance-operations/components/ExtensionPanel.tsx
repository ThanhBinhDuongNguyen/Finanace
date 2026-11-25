"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { type FormEvent } from "react";
import { extensionService, type ExtensionOperation, type OrderSnapshot } from "../../../services/finance/extension.service";
import { apiError } from "../../../services/http";
import { DataDetail } from "../../../components/ui/DataDetail";
import { PayoutAccountPanel } from "./PayoutAccountPanel";

const control = "finance-control";
const button = "finance-button";
type FieldSpec = { name: string; label: string; type?: "text" | "number" | "date" | "month"; optional?: boolean; options?: string[] };
const field = (name: string, label: string, type: FieldSpec["type"] = "text", optional = false): FieldSpec => ({ name, label, type, optional });
const orderField = { name: "orderLineId", label: "Dòng đơn nguồn", options: [] };
const evidence = field("evidence", "Bằng chứng");

function OperationForm({ title, operation, fields, orders, scope, run, disabled }: { title: string; operation: ExtensionOperation; fields: FieldSpec[]; orders: OrderSnapshot[]; scope: { shopCode: string; currency: string }; run: (action: () => Promise<unknown>) => void; disabled: boolean }) {
  const submit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    run(async () => {
      const payload: Record<string, unknown> = { ...scope };
      for (const spec of fields) {
        const value = String(form.get(spec.name) ?? "").trim();
        if (!value && spec.optional) continue;
        payload[spec.name] = spec.type === "number" ? Number(value) : spec.type === "month" ? `${value}-01` : value;
      }
      if (operation === "costs/assessments") payload.isTemporary = payload.isTemporary === "true";
      return extensionService.execute(operation, payload);
    });
  };
  return <details className="rounded border border-slate-200 p-3"><summary className="cursor-pointer text-sm font-medium">{title}</summary><form className="mt-3 grid gap-3 sm:grid-cols-2 lg:grid-cols-3" onSubmit={submit}>{fields.map((spec) => <label key={spec.name} className="text-sm text-slate-600">{spec.label}{spec.name === "orderLineId" ? <select name={spec.name} className={control} required><option value="">Chọn dòng đơn</option>{orders.map((order) => <option key={order.id} value={order.id}>{order.orderId}/{order.externalId} · {order.sku}</option>)}</select> : spec.options ? <select name={spec.name} className={control} required={!spec.optional}>{spec.options.map((value) => <option key={value}>{value}</option>)}</select> : <input name={spec.name} type={spec.type ?? "text"} step={spec.type === "number" ? "0.01" : undefined} className={control} required={!spec.optional} />}</label>)}<button disabled={disabled} className={`${button} self-end`}>Lưu {title.toLowerCase()}</button></form></details>;
}

export function ExtensionPanel({ shopCode, currency, actor, roles }: { shopCode: string; currency: string; actor: string; roles: string[] }) {
  const cache = useQueryClient();
  const can = (...allowed: string[]) => allowed.some((role) => roles.includes(role)) || (allowed.includes("Maker") && roles.includes("Admin"));
  const orders = useQuery({ queryKey: ["finance", "orders", shopCode], queryFn: () => extensionService.orders(shopCode) });
  const refunds = useQuery({ queryKey: ["finance", "refunds", shopCode], queryFn: () => extensionService.refunds(shopCode) });
  const requests = useQuery({ queryKey: ["finance", "refund-requests", shopCode], queryFn: () => extensionService.refundRequests(shopCode) });
  const costs = useQuery({ queryKey: ["finance", "costs", shopCode], queryFn: () => extensionService.costs(shopCode) });
  const returned = useQuery({ queryKey: ["finance", "returns-fees", shopCode], queryFn: () => extensionService.returnsAndFees(shopCode) });
  const documents = useQuery({ queryKey: ["finance", "documents", shopCode], queryFn: () => extensionService.documents(shopCode) });
  const audit = useQuery({ queryKey: ["finance", "audit", shopCode], queryFn: () => extensionService.audit(shopCode) });
  const mutation = useMutation({ mutationFn: (action: () => Promise<unknown>) => action(), onSuccess: () => cache.invalidateQueries({ queryKey: ["finance"] }) });
  const run = (action: () => Promise<unknown>) => mutation.mutate(action);
  const props = { orders: orders.data ?? [], scope: { shopCode, currency }, run, disabled: mutation.isPending };
  const queries = [orders, refunds, requests, costs, returned, documents, audit];
  const failed = queries.find((query) => query.isError);
  return <div className="space-y-4">
    <PayoutAccountPanel shopCode={shopCode} actor={actor} roles={roles} />
    {mutation.isError && <p role="alert" className="text-sm text-red-700">{apiError(mutation.error)}</p>}
    {mutation.isSuccess && <p role="status" className="text-sm text-blue-700">Đã lưu hồ sơ.</p>}
    {failed && <p role="alert" className="text-sm text-amber-700">{apiError(failed.error)}</p>}
    {can("Maker", "Admin") && <>
      <OperationForm {...props} title="Nhập snapshot dòng đơn" operation="orders" fields={[field("externalId", "ID dòng nguồn"), field("orderId", "Mã đơn"), field("legalEntity", "Pháp nhân"), field("channel", "Kênh"), field("sku", "SKU"), field("skuGroup", "Nhóm SKU"), field("quantity", "Số lượng", "number"), field("netAmount", "Giá trị thuần", "number"), field("occurredOn", "Ngày bán", "date"), evidence]} />
      <OperationForm {...props} title="Đề nghị hoàn tiền" operation="refunds/requests" fields={[orderField, field("sourceRequestId", "ID yêu cầu nguồn"), field("amount", "Số tiền đề nghị hoàn", "number"), field("reason", "Lý do hoàn"), evidence]} />
      <OperationForm {...props} title="Nhập xác nhận hoàn từ kênh" operation="refunds/external-confirmations" fields={[orderField, field("sourceRefundId", "ID hoàn từ nguồn"), field("amount", "Số tiền đã hoàn", "number"), field("confirmedOn", "Ngày xác nhận hoàn", "date"), field("approvedRequestId", "ID đề nghị đã duyệt (nếu có)", "text", true), evidence]} />
      <OperationForm {...props} title="Nhập lớp giá vốn nhận hàng" operation="costs/layers" fields={[field("sku", "SKU"), field("sourceReceiptId", "ID chứng từ nhận"), field("quantity", "Lượng nhận", "number"), field("unitCost", "Đơn giá vốn", "number"), field("receivedOn", "Ngày nhận", "date"), evidence]} />
      <OperationForm {...props} title="Ghi nhận phí giao thất bại" operation="failed-delivery-fees" fields={[field("sourceFeeId", "ID phí nguồn"), field("packageId", "ID kiện"), field("amount", "Số tiền phí", "number"), field("occurredOn", "Ngày phát sinh phí", "date"), evidence]} />
    </>}
    {can("Controller", "Admin") && <>
      <OperationForm {...props} title="Điều chỉnh giá vốn hàng trả" operation="returns/cost-adjustments" fields={[orderField, field("sourceQcId", "ID kết quả QC"), { name: "disposition", label: "Định đoạt hàng trả", options: ["Restock", "Scrap"] }, field("quantity", "Lượng trả", "number"), field("postedOn", "Ngày hạch toán điều chỉnh", "date"), evidence]} />
      <OperationForm {...props} title="Chốt phương pháp giá vốn" operation="costs/policies" fields={[field("skuGroup", "Nhóm SKU"), field("period", "Tháng áp dụng", "month"), { name: "method", label: "Phương pháp", options: ["Fifo", "WeightedAverage"] }, evidence]} />
      <OperationForm {...props} title="Tính giá vốn dòng đơn" operation="costs/assessments" fields={[orderField, { name: "isTemporary", label: "Giá vốn tạm", options: ["false", "true"] }, field("estimatedAmount", "Số tiền ước tính (giá vốn tạm)", "number", true), field("postedOn", "Ngày hạch toán giá vốn", "date"), evidence]} />
    </>}
    {requests.data?.map((request) => <details key={request.id} className="rounded border p-3 text-sm"><summary className="cursor-pointer">Yêu cầu {request.sourceRequestId} · {request.status} · {request.amount}</summary><div className="mt-3"><DataDetail value={request} /></div>{can("Checker") && !request.approvedBy && request.requestedBy !== actor && <form className="mt-3 flex gap-2" onSubmit={(event) => { event.preventDefault(); const form = new FormData(event.currentTarget); run(() => extensionService.approveRefund(request.id, String(form.get("evidence")))); }}><input name="evidence" required aria-label={`Bằng chứng duyệt hoàn ${request.sourceRequestId}`} placeholder="Bằng chứng duyệt" className={control} /><button className={button} disabled={mutation.isPending}>Duyệt hoàn</button></form>}</details>)}
    <details className="rounded border p-3"><summary className="cursor-pointer text-sm font-medium">Chứng từ và hóa đơn</summary>{can("Maker") && <form className="mt-3 grid gap-3 sm:grid-cols-2" onSubmit={(event) => { event.preventDefault(); const form = new FormData(event.currentTarget); run(async () => { const file = form.get("file") as File; if (!file?.size || file.size > 5_000_000) throw new Error("Chứng từ cần có nội dung, tối đa 5 MB."); const contentBase64 = await new Promise<string>((resolve, reject) => { const reader = new FileReader(); reader.onload = () => resolve(String(reader.result).split(",")[1]); reader.onerror = () => reject(new Error("Không đọc được file.")); reader.readAsDataURL(file); }); return extensionService.uploadDocument({ shopCode, externalId: String(form.get("externalId")), kind: String(form.get("kind")), sourceId: String(form.get("sourceId")), fileName: file.name, contentBase64, reason: String(form.get("reason")) }); }); }}><label className="text-sm">Mã chứng từ<input name="externalId" className={control} required /></label><label className="text-sm">ID nguồn đơn / refund / journal<input name="sourceId" className={control} required /></label><label className="text-sm">Loại chứng từ<select name="kind" className={control}><option>Invoice</option><option>CreditNote</option><option>SupportingDocument</option><option>ReturnEvidence</option></select></label><label className="text-sm">Lý do / thay đổi phiên bản<input name="reason" className={control} required /></label><label className="text-sm">File chứng từ<input name="file" type="file" className="mt-1 block text-sm" required /></label><button className={`${button} self-end`} disabled={mutation.isPending}>Lưu phiên bản chứng từ</button></form>}{documents.data?.map((document) => <div key={document.id} className="mt-4 border-t pt-3 text-sm"><p>{document.externalId} · {document.kind}</p>{document.revisions.map((revision) => <div key={revision.revisionId} className="mt-2 space-y-2"><p>v{revision.version} · {revision.fileName} · {revision.status}</p><button type="button" className={button} disabled={mutation.isPending} onClick={() => run(async () => { const blob = await extensionService.downloadDocument(revision.revisionId); const url = URL.createObjectURL(blob), link = window.document.createElement("a"); link.href = url; link.download = revision.fileName; link.click(); setTimeout(() => URL.revokeObjectURL(url), 1000); })}>Tải chứng từ</button>{can("Checker", "Controller") && revision.uploadedBy !== actor && revision.status === "PendingReview" && <form className="flex gap-2" onSubmit={(event) => { event.preventDefault(); const form = new FormData(event.currentTarget); run(() => extensionService.reviewDocument(revision.revisionId, { approve: String(form.get("approve")) === "true", note: String(form.get("note")) })); }}><input name="note" aria-label={`Nhận xét phiên bản ${revision.version}`} required className={control} /><select name="approve" className={control}><option value="true">Xác minh</option><option value="false">Từ chối</option></select><button className={button} disabled={mutation.isPending}>Duyệt chứng từ</button></form>}</div>)}</div>)}</details>
    {[{ label: "Snapshot đơn", value: orders.data }, { label: "Xác nhận hoàn", value: refunds.data }, { label: "Giá vốn và allocation", value: costs.data }, { label: "Hàng trả và phí giao thất bại", value: returned.data }, { label: "Audit nghiệp vụ", value: audit.data }].map((item) => <details className="text-sm" key={item.label}><summary className="cursor-pointer">{item.label}</summary><div className="mt-3"><DataDetail value={item.value} /></div></details>)}
  </div>;
}

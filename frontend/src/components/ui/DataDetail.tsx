import type { ReactNode } from "react";

const labels: Record<string, string> = {
  id: "Mã hồ sơ", externalId: "Mã nguồn", shopCode: "Cửa hàng", currency: "Tiền tệ", amount: "Số tiền",
  occurredOn: "Ngày nghiệp vụ", businessType: "Loại nghiệp vụ", rawData: "Dòng dữ liệu gốc", status: "Trạng thái",
  validationError: "Lý do cần xử lý", sourceKey: "Khóa nguồn", importBatchId: "Batch nhập", importLineId: "Dòng nguồn",
  referenceType: "Loại tham chiếu", referenceId: "Tham chiếu nguồn", period: "Kỳ", postedBy: "Người ghi sổ", postedAt: "Thời điểm ghi sổ",
  reason: "Lý do", evidence: "Bằng chứng", lines: "Dòng chi tiết", accountCode: "Tài khoản", debit: "Nợ", credit: "Có",
  reversesJournalEntryId: "Bút toán gốc", payoutId: "Payout", bankTransactionId: "Giao dịch ngân hàng", difference: "Chênh lệch",
  tolerance: "Dung sai", proposedBy: "Người đề xuất", reviewedBy: "Người duyệt", createdAt: "Ngày tạo", statementAllocations: "Phân bổ statement",
  bankAllocations: "Phân bổ ngân hàng", orderLineId: "Dòng đơn", orderId: "Mã đơn", quantity: "Số lượng", netAmount: "Giá trị thuần",
  sku: "SKU", skuGroup: "Nhóm SKU", legalEntity: "Pháp nhân", channel: "Kênh", sourceRefundId: "Mã hoàn nguồn",
  sourceRequestId: "Mã yêu cầu nguồn", requestedBy: "Người yêu cầu", approvedBy: "Người phê duyệt", confirmedBy: "Người xác nhận",
  approvedAt: "Thời điểm duyệt", confirmedOn: "Ngày xác nhận", recordedAt: "Ngày ghi nhận", journalEntryId: "Bút toán liên quan",
  method: "Phương pháp", unitCost: "Đơn giá vốn", usedQuantity: "Lượng đã phân bổ", receivedOn: "Ngày nhận", sourceReceiptId: "Chứng từ nhận",
  policies: "Chính sách", layers: "Lớp giá vốn", assessments: "Kết quả giá vốn", allocations: "Phân bổ", returns: "Hàng trả", fees: "Phí",
  adjustment: "Điều chỉnh", isTemporary: "Giá vốn tạm", sourceQcId: "Mã QC", disposition: "Định đoạt", sourceFeeId: "Mã phí nguồn", packageId: "Mã kiện",
  action: "Thao tác", actor: "Người thực hiện", occurredAt: "Thời điểm", sourceId: "Nguồn liên quan", version: "Phiên bản",
  sourceType: "Loại nguồn", sourceBusinessType: "Loại nghiệp vụ nguồn", targetCategory: "Nhóm đích", category: "Phân loại",
  effectiveFrom: "Hiệu lực từ", effectiveTo: "Hiệu lực đến", classificationMappingId: "Mapping áp dụng", classifiedBy: "Người phân loại",
  classifiedAt: "Thời điểm phân loại", createdBy: "Người tạo", owner: "Người phụ trách", dueOn: "Hạn xử lý", reasonCode: "Nguyên nhân",
  note: "Ghi chú", sourceMode: "Nguồn đang ghi", checksum: "Mã kiểm tra", importedBy: "Người nhập", importedAt: "Thời điểm nhập",
};

const statuses: Record<string, { text: string; tone: string }> = {
  Open: { text: "Đang xử lý", tone: "info" }, WaitingExternal: { text: "Chờ bên ngoài", tone: "warning" }, PendingApproval: { text: "Chờ duyệt", tone: "warning" },
  Closed: { text: "Đã đóng", tone: "success" }, Candidate: { text: "Chờ đối soát", tone: "info" }, Disputed: { text: "Có chênh lệch", tone: "danger" },
  FullyMatched: { text: "Đã đối soát", tone: "success" }, Valid: { text: "Hợp lệ", tone: "success" }, Quarantined: { text: "Cần xử lý", tone: "danger" },
  Posted: { text: "Đã ghi sổ", tone: "success" }, Reversed: { text: "Đã đảo", tone: "neutral" }, Unmapped: { text: "Chưa phân loại", tone: "warning" },
  Classified: { text: "Đã phân loại", tone: "success" }, Approved: { text: "Đã duyệt", tone: "success" }, Requested: { text: "Đã đề nghị", tone: "info" },
  Confirmed: { text: "Đã xác nhận", tone: "success" }, Verified: { text: "Đã xác minh", tone: "success" }, PendingReview: { text: "Chờ kiểm tra", tone: "warning" },
  Rejected: { text: "Từ chối", tone: "danger" }, Temporary: { text: "Tạm tính", tone: "warning" }, Final: { text: "Chính thức", tone: "success" },
};

export function StatusBadge({ status }: { status: string }) {
  const value = statuses[status];
  return <span className="finance-status" data-tone={value?.tone === "success" ? "positive" : value?.tone ?? "neutral"} title={status}>{value?.text ?? status}</span>;
}

function label(key: string) { return labels[key] ?? key.replace(/([a-z])([A-Z])/g, "$1 $2"); }
function isRecord(value: unknown): value is Record<string, unknown> { return !!value && typeof value === "object" && !Array.isArray(value); }

function valueNode(value: unknown, key: string, depth: number): ReactNode {
  if (value === null || value === undefined || value === "") return <span className="text-slate-400">—</span>;
  if (typeof value === "boolean") return value ? "Có" : "Không";
  if (typeof value === "number") return <span className="tabular-nums">{new Intl.NumberFormat("vi-VN", { maximumFractionDigits: 4 }).format(value)}</span>;
  if (typeof value === "string") {
    if (key === "status") return <StatusBadge status={value} />;
    if (/^\d{4}-\d{2}-\d{2}(T|$)/.test(value)) {
      const date = new Date(value.includes("T") ? value : `${value}T00:00:00`);
      if (!Number.isNaN(date.valueOf())) return value.includes("T") ? date.toLocaleString("vi-VN") : date.toLocaleDateString("vi-VN");
    }
    return <span className={key.toLowerCase().endsWith("id") || key === "checksum" ? "break-all font-mono text-xs" : "break-words"}>{value}</span>;
  }
  if (depth > 4) return <span className="text-slate-500">Xem hồ sơ nguồn để kiểm tra thêm.</span>;
  return <DataDetail value={value} depth={depth + 1} />;
}

/** Labelled source data, shared by operations, audit and journal drill-down. */
export function DataDetail({ value, depth = 0, emptyMessage = "Chưa có dữ liệu chi tiết." }: { value: unknown; depth?: number; emptyMessage?: string }) {
  if (value == null || Array.isArray(value) && value.length === 0) return <p className="finance-empty-inline">{emptyMessage}</p>;
  if (Array.isArray(value)) {
    if (!value.every(isRecord)) return <ul className="space-y-2">{value.map((item, index) => <li key={index}>{valueNode(item, "", depth)}</li>)}</ul>;
    const keys = [...new Set(value.flatMap(item => Object.keys(item)))];
    const priority = ["externalId", "referenceId", "accountCode", "orderId", "sku", "businessType", "category", "amount", "debit", "credit", "quantity", "status", "occurredOn", "action", "actor", "note"];
    const shown = [...priority.filter(key => keys.includes(key)), ...keys.filter(key => !priority.includes(key) && key !== "rawData" && key !== "sourceKey" && key !== "id" && !key.endsWith("Id"))].slice(0, 7);
    if (shown.length === 0 && keys.length > 0) shown.push(keys[0]);
    const remaining = keys.filter(key => !shown.includes(key));
    return <div className="finance-table-scroll" role="region" aria-label="Dữ liệu chi tiết" tabIndex={0}><table className="finance-table"><thead><tr>{shown.map(key => <th key={key} scope="col">{label(key)}</th>)}{remaining.length > 0 && <th scope="col">Thông tin bổ sung</th>}</tr></thead><tbody>{value.map((row, index) => <tr key={typeof row.id === "string" ? row.id : index}>{shown.map(key => <td key={key}>{valueNode(row[key], key, depth)}</td>)}{remaining.length > 0 && <td><details className="finance-detail"><summary>Xem thêm</summary><DataDetail depth={depth + 1} value={Object.fromEntries(remaining.map(key => [key, row[key]]))} /></details></td>}</tr>)}</tbody></table></div>;
  }
  if (!isRecord(value)) return <p className="text-sm">{valueNode(value, "", depth)}</p>;
  return <dl className="finance-data-list">{Object.entries(value).map(([key, item]) => <div key={key} className="finance-data-row"><dt>{label(key)}</dt><dd>{valueNode(item, key, depth)}</dd></div>)}</dl>;
}

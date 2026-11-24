"use client";

import { useQuery } from "@tanstack/react-query";
import { operationsService } from "../../../services/finance/operations.service";

export function OperationsSummary() {
  const { data, isLoading, isError } = useQuery({ queryKey: ["finance", "operations-summary"], queryFn: operationsService.summary });
  if (isLoading) return <p className="text-sm text-slate-500">Đang tải chỉ số vận hành…</p>;
  if (isError || !data) return <p className="text-sm text-amber-700">Chưa kết nối API vận hành. Biểu đồ bên dưới vẫn là dữ liệu minh hoạ.</p>;
  const items = [["Batch import", data.importBatchCount], ["Case disputed", data.disputedCaseCount], ["Ngoại lệ mở", data.openExceptionCount], ["Journal đã post", data.postedJournalCount]];
  return <div className="grid grid-cols-2 gap-3 xl:grid-cols-4">{items.map(([label, value]) => <div key={String(label)} className="rounded-lg border border-sky-100 bg-sky-50 p-4"><p className="text-sm text-slate-600">{label}</p><p className="mt-1 text-2xl font-bold text-[#17375e]">{value}</p></div>)}</div>;
}

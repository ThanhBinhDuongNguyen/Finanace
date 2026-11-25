"use client";

import { useQuery } from "@tanstack/react-query";
import { operationsService } from "../../../services/finance/operations.service";
import Link from "next/link";
import { MetricCard } from "../../../components/ui/MetricCard";

export function OperationsSummary() {
  const { data, isLoading, isError, refetch, isFetching } = useQuery({ queryKey: ["finance", "operations-summary"], queryFn: operationsService.summary, retry: false });
  if (isLoading) return <section aria-label="Tổng hợp vận hành" aria-busy="true"><p role="status" className="mb-3 text-sm text-slate-500">Đang tải chỉ số vận hành…</p><div className="grid grid-cols-2 gap-3 xl:grid-cols-4">{[0, 1, 2, 3].map((item) => <div key={item} className="h-24 animate-pulse rounded-xl bg-slate-200" />)}</div></section>;
  if (isError || !data) return <section className="rounded-xl border border-slate-200 bg-white p-4" aria-label="Kết nối dữ liệu vận hành"><p className="text-sm font-medium text-slate-800">Chưa tải được dữ liệu vận hành</p><p className="mt-1 text-sm text-slate-500">Đăng nhập để xem chỉ số từ hệ thống. Các báo cáo minh họa vẫn có thể xem bên dưới.</p><div className="mt-3 flex items-center gap-4 text-sm"><Link href="/van-hanh-finance" className="font-medium text-[#17375e] underline underline-offset-4">Mở Vận hành Finance</Link><button type="button" disabled={isFetching} className="font-medium text-slate-600 underline underline-offset-4 disabled:opacity-50" onClick={() => void refetch()}>{isFetching ? "Đang thử lại…" : "Thử lại kết nối"}</button></div></section>;
  const items = [{ label: "Batch import", value: data.importBatchCount }, { label: "Case chênh lệch", value: data.disputedCaseCount }, { label: "Ngoại lệ mở", value: data.openExceptionCount }, { label: "Journal đã post", value: data.postedJournalCount }];
  return <section aria-labelledby="operations-summary-heading"><div className="mb-3 flex items-center justify-between gap-3"><h2 id="operations-summary-heading" className="text-base font-semibold text-slate-900">Tình hình vận hành</h2><span className="text-xs font-medium text-emerald-700">Dữ liệu từ API</span></div><div className="grid grid-cols-2 gap-3 xl:grid-cols-4">{items.map((item) => <MetricCard key={item.label} label={item.label} value={new Intl.NumberFormat("vi-VN").format(item.value)} />)}</div></section>;
}

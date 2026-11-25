"use client";

import { useMutation, useQuery } from "@tanstack/react-query";
import { reportService } from "../../../services/finance/report.service";
import { apiError } from "../../../services/http";
import { DataDetail } from "../../../components/ui/DataDetail";

export function LedgerReportPanel({ shopCode, currency, period }: { shopCode: string; currency: string; period: string }) {
  const report = useQuery({ queryKey: ["finance", "ledger-report", shopCode, currency, period], queryFn: () => reportService.ledger({ shopCode, currency, period: `${period}-01` }), enabled: !!period && !!shopCode });
  const download = useMutation({ mutationFn: () => reportService.exportJournals(shopCode), onSuccess: (blob) => {
    const url = URL.createObjectURL(blob), link = document.createElement("a");
    link.href = url; link.download = `journals-${shopCode}.csv`; link.click();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  } });
  if (!period) return <p className="text-sm text-slate-500">Chọn kỳ báo cáo để xem tổng hợp từ journal.</p>;
  if (report.isLoading) return <p role="status">Đang tổng hợp journal…</p>;
  if (report.isError) return <p role="alert" className="text-sm text-red-700">{apiError(report.error)}</p>;
  const data = report.data;
  if (!data) return null;
  return <div className="space-y-3"><p className="text-sm text-slate-500">Nguồn: {data.sourceStatus} · Phiên bản {data.definitionVersion} · Snapshot {new Date(data.snapshotAt).toLocaleString("vi-VN")} · Chưa xác định: {data.unknownCount}</p>{data.sourceStatus === "Missing" ? <p className="rounded bg-amber-50 p-3 text-sm text-amber-800">Kỳ này chưa có journal. Chưa đủ nguồn để tính chỉ số.</p> : <div className="grid gap-3 sm:grid-cols-3">{[["Doanh thu thuần theo sổ", data.netRevenue], ["Lợi nhuận quản trị theo sổ", data.managementProfit], ["Tiền theo sổ", data.receivedCash]].map(([label, value]) => <div key={String(label)} className="rounded bg-slate-50 p-3"><p className="text-xs text-slate-500">{label}</p><p className="mt-1 font-semibold">{typeof value === "number" ? new Intl.NumberFormat("vi-VN").format(value) : value} {currency}</p></div>)}</div>}<p className="text-xs text-slate-500">Số liệu phụ thuộc journal đã ghi nhận và phạm vi tài khoản. Truy dòng nguồn để kiểm tra các khoản chưa đủ chứng từ.</p><button type="button" className="finance-button" disabled={download.isPending} onClick={() => download.mutate()}>Xuất journal trong scope shop</button>{download.isError && <p role="alert" className="text-sm text-red-700">{apiError(download.error)}</p>}<details className="text-sm"><summary className="cursor-pointer">Chi tiết journal nguồn ({data.entries.length})</summary><div className="mt-3"><DataDetail value={data.entries} /></div></details></div>;
}

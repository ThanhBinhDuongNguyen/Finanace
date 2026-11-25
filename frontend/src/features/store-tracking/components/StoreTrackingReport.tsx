"use client";

import { ReportFrame } from "../../../components/ui/ReportFrame";
import { MetricCard } from "../../../components/ui/MetricCard";

import { type ReactElement, useMemo, useState } from "react";
import { Bar, BarChart, CartesianGrid, ComposedChart, Legend, Line, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";

type MetricKind = "money" | "percent" | "count" | "ratio";
type Aggregation = "sum" | "average";
type MetricRow = { label: string; kind: MetricKind; aggregation: Aggregation; values: number[] };
type GroupRow = { label: string; type: "group" };
type TableRow = MetricRow | GroupRow;

const months = ["01/2026", "02/2026", "03/2026", "04/2026", "05/2026", "06/2026", "07/2026", "08/2026", "09/2026", "10/2026", "11/2026", "12/2026"];
const storeOptions = [
  { id: "all", label: "Toàn chuỗi", factor: 4, rateShift: 0 },
  { id: "ch01", label: "CH01 – Q1 – Nguyễn Trãi", factor: 1, rateShift: 0 },
  { id: "ch02", label: "CH02 – Q7 – Crescent Mall", factor: 0.77, rateShift: 2.2 },
  { id: "ch03", label: "CH03 – Thủ Đức – Vincom", factor: 0.78, rateShift: 0.6 },
  { id: "ch04", label: "CH04 – Hà Nội – Phố Huế", factor: 0.64, rateShift: 2.8 },
  { id: "ch05", label: "CH05 – Đà Nẵng – Lê Duẩn", factor: 0.34, rateShift: -4.5 },
] as const;

const baseRevenue = [2632.6, 2047.3, 2344.1, 2331.9, 2300.8, 2208.6, 2533.8, 2392.3, 2478.2, 2544.3, 2917.3, 3498];
const baseCogs = [1418, 1088.5, 1238, 1263, 1251.9, 1177.7, 1349.2, 1282.2, 1343.2, 1354.3, 1561, 1853];
const baseOperating = [499.1, 474, 499, 491.8, 482.6, 485.4, 504.6, 496.4, 495.6, 497.6, 521, 548.1];
const baseFootfall = [20482, 15428, 18513, 17804, 16339, 16227, 18920, 16587, 20561, 19927, 21352, 30559];
const baseBills = [4415, 3182, 3256, 3750, 3558, 3439, 3693, 3538, 3818, 3976, 4463, 5238];
const baseConversion = [21.6, 20.6, 17.6, 21.1, 21.8, 21.2, 19.5, 21.3, 18.6, 20, 20.9, 17.1];
const baseBill = [596, 643, 720, 622, 647, 642, 686, 676, 649, 640, 654, 668];
const baseProducts = [1.54, 1.6, 1.64, 1.67, 1.69, 1.53, 1.64, 1.69, 1.69, 1.71, 1.78, 1.67];
const baseM2 = [14.63, 11.37, 13.02, 12.96, 12.78, 12.27, 14.08, 13.29, 13.77, 14.13, 16.21, 19.43];

function format(value: number, kind: MetricKind) {
  if (kind === "percent") return `${value.toFixed(1).replace(".", ",")}%`;
  if (kind === "ratio") return value.toFixed(2).replace(".", ",");
  const rounded = kind === "count" ? Math.round(value) : value;
  return new Intl.NumberFormat("vi-VN", { maximumFractionDigits: kind === "count" ? 0 : 1 }).format(rounded);
}

function growth(from: number, to: number) {
  const value = ((to - from) / from) * 100;
  return `${value >= 0 ? "+" : ""}${value.toFixed(1).replace(".", ",")}%`;
}

export function StoreTrackingReport() {
  const [storeId, setStoreId] = useState<(typeof storeOptions)[number]["id"]>("ch01");
  const [fromIndex, setFromIndex] = useState(0);
  const [toIndex, setToIndex] = useState(11);
  const store = storeOptions.find((item) => item.id === storeId) ?? storeOptions[1];
  const indices = Array.from({ length: toIndex - fromIndex + 1 }, (_, index) => fromIndex + index);

  const data = useMemo(() => {
    const scaled = (values: number[], kind: MetricKind) => values.map((value) => kind === "percent" ? value + store.rateShift : kind === "ratio" ? value + store.rateShift / 100 : value * store.factor);
    const revenue = scaled(baseRevenue, "money");
    const cogs = scaled(baseCogs, "money");
    const grossProfit = revenue.map((value, index) => value - cogs[index]);
    const grossMargin = grossProfit.map((value, index) => (value / revenue[index]) * 100);
    const operating = scaled(baseOperating, "money");
    const ebitda = grossProfit.map((value, index) => value - operating[index]);
    const depreciation = Array(12).fill(45 * store.factor);
    const profit = ebitda.map((value, index) => value - depreciation[index]);
    const profitMargin = profit.map((value, index) => (value / revenue[index]) * 100);
    const footfall = scaled(baseFootfall, "count");
    const bills = scaled(baseBills, "count");
    const conversion = scaled(baseConversion, "percent");
    const averageBill = scaled(baseBill, "money");
    const products = scaled(baseProducts, "ratio");
    const revenuePerVisit = revenue.map((value, index) => (value * 1000) / footfall[index]);
    const revenuePerM2 = scaled(baseM2, "money");
    return { revenue, cogs, grossProfit, grossMargin, operating, ebitda, depreciation, profit, profitMargin, footfall, bills, conversion, averageBill, products, revenuePerVisit, revenuePerM2 };
  }, [store]);

  const rows: TableRow[] = [
    { type: "group", label: "LÃI LỖ" },
    { label: "Doanh thu thuần", kind: "money", aggregation: "sum", values: data.revenue }, { label: "Giá vốn", kind: "money", aggregation: "sum", values: data.cogs },
    { label: "Lãi gộp", kind: "money", aggregation: "sum", values: data.grossProfit }, { label: "Biên lãi gộp", kind: "percent", aggregation: "average", values: data.grossMargin },
    { label: "Chi phí vận hành", kind: "money", aggregation: "sum", values: data.operating }, { label: "EBITDA cửa hàng", kind: "money", aggregation: "sum", values: data.ebitda },
    { label: "Khấu hao", kind: "money", aggregation: "sum", values: data.depreciation }, { label: "LỢI NHUẬN CỬA HÀNG", kind: "money", aggregation: "sum", values: data.profit },
    { label: "Biên lợi nhuận", kind: "percent", aggregation: "average", values: data.profitMargin },
    { type: "group", label: "KHÁCH & BÁN HÀNG" },
    { label: "Footfall", kind: "count", aggregation: "sum", values: data.footfall }, { label: "Số bill", kind: "count", aggregation: "sum", values: data.bills },
    { label: "Tỷ lệ chuyển đổi", kind: "percent", aggregation: "average", values: data.conversion }, { label: "Giá trị bill TB (nghìn đ)", kind: "money", aggregation: "average", values: data.averageBill },
    { label: "SP / bill", kind: "ratio", aggregation: "average", values: data.products }, { label: "Doanh thu / lượt khách (nghìn đ)", kind: "money", aggregation: "average", values: data.revenuePerVisit },
    { label: "Doanh thu / m²/tháng (tr.đ)", kind: "money", aggregation: "average", values: data.revenuePerM2 },
  ];
  const monthly = indices.map((index) => ({ month: months[index], revenue: data.revenue[index], grossMargin: data.grossMargin[index], profit: data.profit[index], footfall: data.footfall[index], conversion: data.conversion[index], averageBill: data.averageBill[index], products: data.products[index] }));
  const selected = <T,>(values: T[]) => indices.map((index) => values[index]);
  const totals = (row: MetricRow) => { const values = selected(row.values); const sum = values.reduce((total, value) => total + value, 0); return row.aggregation === "sum" ? sum : sum / values.length; };
  const rangeLabel = `${months[fromIndex]} → ${months[toIndex]}`;
  const kpis = [{ label: "Doanh thu", values: data.revenue }, { label: "LN cửa hàng", values: data.profit }, { label: "Footfall", values: data.footfall }, { label: "Chuyển đổi", values: data.conversion }, { label: "Bill TB", values: data.averageBill }];

  return <div className="space-y-5"><section className="finance-filter-bar"><label className="flex items-center gap-2">Cửa hàng:<select value={storeId} onChange={(event) => setStoreId(event.target.value as typeof storeId)} className="rounded border border-slate-300 px-2 py-1"><option value="all">Toàn chuỗi</option>{storeOptions.slice(1).map((item) => <option key={item.id} value={item.id}>{item.label}</option>)}</select></label><label className="flex items-center gap-2">Từ tháng:<select value={fromIndex} onChange={(event) => setFromIndex(Math.min(Number(event.target.value), toIndex))} className="rounded border border-slate-300 px-2 py-1">{months.map((month, index) => <option key={month} value={index}>{month}</option>)}</select></label><label className="flex items-center gap-2">Đến tháng:<select value={toIndex} onChange={(event) => setToIndex(Math.max(Number(event.target.value), fromIndex))} className="rounded border border-slate-300 px-2 py-1">{months.map((month, index) => <option key={month} value={index}>{month}</option>)}</select></label><span className="text-sm text-slate-500">Đơn vị: triệu đồng</span></section><div><p className="mb-3 text-sm text-slate-500">Biến động trong kỳ {rangeLabel}</p><div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-5">{kpis.map((kpi) => <MetricCard key={kpi.label} label={kpi.label} value={growth(kpi.values[fromIndex], kpi.values[toIndex])} tone={kpi.values[toIndex] >= kpi.values[fromIndex] ? "positive" : "warning"} />)}</div></div><ReportFrame title="Lãi lỗ và hiệu quả theo tháng" description="Đơn vị tài chính: triệu đồng; tỷ lệ và số lượng theo tiêu đề cột." scrollHint><div className="finance-table-scroll" tabIndex={0} role="region" aria-label="Bảng hiệu quả cửa hàng có thể cuộn ngang"><table className="finance-table min-w-[1240px]"><caption className="sr-only">Lãi lỗ và hiệu quả cửa hàng theo tháng</caption><thead className="bg-[#203f70] text-white"><tr><th scope="col" className="sticky left-0 z-10 min-w-[var(--report-label-width)] bg-[#203f70] px-2 py-1.5 text-center">Chỉ tiêu</th>{indices.map((index) => <th scope="col" key={months[index]} className="min-w-20 px-2 py-1.5">{months[index]}</th>)}<th scope="col" className="min-w-24 px-2 py-1.5">Cả kỳ</th></tr></thead><tbody>{rows.map((row) => "type" in row ? <tr key={row.label} className="finance-row-group bg-[#d9e5f1] font-bold text-[#17375e]"><td className="sticky left-0 z-10 min-w-[var(--report-label-width)] bg-[#d9e5f1] px-2 py-1 text-left">{row.label}</td><td colSpan={indices.length + 1} className="bg-[#d9e5f1]" /></tr> : <tr key={row.label} className={`border-b border-slate-200 ${row.label === "LỢI NHUẬN CỬA HÀNG" || row.label === "Lãi gộp" || row.label === "EBITDA cửa hàng" || row.label === "Doanh thu thuần" || row.label === "Footfall" || row.label === "Tỷ lệ chuyển đổi" || row.label === "Giá trị bill TB (nghìn đ)" ? "font-bold" : ""} ${row.kind === "percent" ? "italic text-slate-600" : ""}`}><td className="sticky left-0 z-10 bg-white px-2 py-1 text-left whitespace-nowrap">{row.label}</td>{selected(row.values).map((value, index) => <td key={`${row.label}-${index}`} className="px-2 py-1 whitespace-nowrap">{format(value, row.kind)}</td>)}<td className="px-2 py-1 font-semibold whitespace-nowrap">{format(totals(row), row.kind)}</td></tr>)}</tbody></table></div></ReportFrame><div className="grid gap-5 2xl:grid-cols-3"><ChartCard title="Doanh thu & biên lợi nhuận"><ComposedChart data={monthly}><CartesianGrid stroke="#d7dde5" vertical={false} /><XAxis dataKey="month" angle={-45} textAnchor="end" height={55} tick={{ fontSize: 11 }} /><YAxis yAxisId="left" /><YAxis yAxisId="right" orientation="right" unit="%" /><Tooltip /><Legend verticalAlign="top" height={30} /><Bar isAnimationActive={false} yAxisId="left" dataKey="revenue" name="Doanh thu" fill="#203f70" /><Line isAnimationActive={false} yAxisId="right" dataKey="grossMargin" name="Biên LN %" stroke="#ee7d19" strokeWidth={3} /></ComposedChart></ChartCard><ChartCard title="Lợi nhuận cửa hàng"><BarChart data={monthly}><CartesianGrid stroke="#d7dde5" vertical={false} /><XAxis dataKey="month" angle={-45} textAnchor="end" height={55} tick={{ fontSize: 11 }} /><YAxis /><Tooltip /><Bar isAnimationActive={false} dataKey="profit" name="Lợi nhuận" fill="#0aa3a1" /></BarChart></ChartCard><ChartCard title="Footfall & tỷ lệ chuyển đổi"><ComposedChart data={monthly}><CartesianGrid stroke="#d7dde5" vertical={false} /><XAxis dataKey="month" angle={-45} textAnchor="end" height={55} tick={{ fontSize: 11 }} /><YAxis yAxisId="left" /><YAxis yAxisId="right" orientation="right" unit="%" /><Tooltip /><Legend verticalAlign="top" height={30} /><Bar isAnimationActive={false} yAxisId="left" dataKey="footfall" name="Footfall" fill="#347cba" /><Line isAnimationActive={false} yAxisId="right" dataKey="conversion" name="Chuyển đổi %" stroke="#ee7d19" strokeWidth={3} /></ComposedChart></ChartCard><ChartCard title="Giá trị bill TB & SP/bill"><ComposedChart data={monthly}><CartesianGrid stroke="#d7dde5" vertical={false} /><XAxis dataKey="month" angle={-45} textAnchor="end" height={55} tick={{ fontSize: 11 }} /><YAxis yAxisId="left" /><YAxis yAxisId="right" orientation="right" /><Tooltip /><Legend verticalAlign="top" height={30} /><Bar isAnimationActive={false} yAxisId="left" dataKey="averageBill" name="Bill TB (nghìn đ)" fill="#8d43b4" /><Line isAnimationActive={false} yAxisId="right" dataKey="products" name="SP/bill" stroke="#ee7d19" strokeWidth={3} /></ComposedChart></ChartCard></div></div>;
}

function ChartCard({ title, children }: { title: string; children: ReactElement }) {
  return <ReportFrame title={title}><div className="finance-chart"><ResponsiveContainer width="100%" height="100%">{children}</ResponsiveContainer></div></ReportFrame>;
}

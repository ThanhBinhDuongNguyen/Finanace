type MetricUnit = "percent" | "ratio" | "number";
type Comparison = "gte" | "lte" | "neutral";

type GroupRow = { type: "group"; label: string };
type MetricRow = { type: "metric"; label: string; thresholdLabel: string; threshold: number; comparison: Comparison; unit: MetricUnit; values: number[] };
type HealthRow = GroupRow | MetricRow;

const months = ["01/2026", "02/2026", "03/2026", "04/2026", "05/2026", "06/2026", "07/2026", "08/2026", "09/2026", "10/2026", "11/2026", "12/2026"];

const rows: HealthRow[] = [
  { type: "group", label: "KHẢ NĂNG SINH LỜI" },
  { type: "metric", label: "Biên lãi gộp", thresholdLabel: "≥ 40%", threshold: 40, comparison: "gte", unit: "percent", values: [45.9, 45.9, 46.1, 45.6, 45.9, 46, 46, 45.6, 45.8, 46, 45.9, 46.1] },
  { type: "metric", label: "Biên EBITDA", thresholdLabel: "≥ 8%", threshold: 8, comparison: "gte", unit: "percent", values: [12.9, 3.6, 7, 7.8, 8.6, 8.3, 9, 9.9, 10, 12.2, 13.9, 17.8] },
  { type: "metric", label: "Biên LN ròng", thresholdLabel: "≥ 4%", threshold: 4, comparison: "gte", unit: "percent", values: [8, -0.5, 2.7, 3.4, 4.2, 3.8, 4.5, 5.4, 5.5, 7.5, 9.1, 12.6] },
  { type: "metric", label: "ROE (năm hóa)", thresholdLabel: "≥ 15%", threshold: 15, comparison: "gte", unit: "percent", values: [125.7, -5.3, 31.8, 40.1, 49.3, 42.4, 50.2, 60.9, 58.3, 81.5, 99.5, 151.2] },
  { type: "group", label: "THANH KHOẢN" },
  { type: "metric", label: "Thanh toán hiện hành", thresholdLabel: "≥ 1,2x", threshold: 1.2, comparison: "gte", unit: "ratio", values: [1.53, 1.65, 1.64, 1.62, 1.65, 1.7, 1.79, 1.81, 1.82, 1.85, 1.89, 1.89] },
  { type: "metric", label: "Thanh toán nhanh", thresholdLabel: "≥ 0,5x", threshold: 0.5, comparison: "gte", unit: "ratio", values: [0.45, 0.67, 0.66, 0.59, 0.64, 0.67, 0.61, 0.6, 0.66, 0.61, 0.62, 0.57] },
  { type: "metric", label: "Số tháng chi phí tiền mặt đủ trả", thresholdLabel: "≥ 2", threshold: 2, comparison: "gte", unit: "number", values: [1.3, 1.9, 1.9, 1.7, 1.9, 2, 1.6, 1.6, 1.8, 1.7, 1.8, 1.7] },
  { type: "group", label: "ĐÒN BẨY" },
  { type: "metric", label: "Nợ / Vốn CSH", thresholdLabel: "≤ 1,0x", threshold: 1, comparison: "lte", unit: "ratio", values: [1.04, 1.04, 1, 0.96, 0.91, 0.87, 0.73, 0.69, 0.65, 0.6, 0.54, 0.47] },
  { type: "metric", label: "EBIT / lãi vay", thresholdLabel: "≥ 3x", threshold: 3, comparison: "gte", unit: "ratio", values: [16.76, 0.43, 5.33, 6.68, 8.33, 7.62, 9.24, 12.83, 13.07, 19.12, 25.2, 42.44] },
  { type: "group", label: "VỐN LƯU ĐỘNG" },
  { type: "metric", label: "Số ngày tồn kho", thresholdLabel: "≤ 75", threshold: 75, comparison: "lte", unit: "number", values: [76, 79, 76, 77, 76, 78, 81, 82, 79, 82, 81, 81] },
  { type: "metric", label: "Số ngày phải thu", thresholdLabel: "≤ 15", threshold: 15, comparison: "lte", unit: "number", values: [2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 2, 3] },
  { type: "metric", label: "Số ngày phải trả NCC", thresholdLabel: "≥ 30", threshold: 30, comparison: "gte", unit: "number", values: [42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42] },
  { type: "metric", label: "Chu kỳ tiền mặt (ngày)", thresholdLabel: "thấp tốt", threshold: 0, comparison: "neutral", unit: "number", values: [36, 39, 36, 38, 36, 38, 41, 42, 39, 43, 42, 41] },
  { type: "metric", label: "Tồn kho > 90 ngày", thresholdLabel: "≤ 15%", threshold: 15, comparison: "lte", unit: "percent", values: [14, 14.4, 14.5, 14.9, 14.6, 15, 16, 16.4, 16.3, 16.4, 16.8, 17.1] },
];

function formatValue(value: number, unit: MetricUnit) {
  if (unit === "percent") return `${value.toFixed(1).replace(".", ",")}%`;
  if (unit === "ratio") return `${value.toFixed(2).replace(".", ",")}x`;
  return Number.isInteger(value) ? String(value) : value.toFixed(1).replace(".", ",");
}

function statusClass(row: MetricRow, value: number) {
  const passed = row.comparison === "gte" ? value >= row.threshold : row.comparison === "lte" ? value <= row.threshold : undefined;
  return passed === undefined ? "" : passed ? "bg-[#dff2e3]" : "bg-[#f9dedd]";
}

export function FinancialHealthReport() {
  return <section className="overflow-hidden rounded-xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5"><h2 className="mb-3 text-base font-bold text-[#17375e]">Chỉ số sức khỏe tài chính (xanh = đạt ngưỡng, đỏ = chưa đạt)</h2><div className="overflow-x-auto"><table className="min-w-[1320px] w-full border-collapse text-right text-sm"><thead className="bg-[#203f70] text-white"><tr><th className="sticky left-0 z-10 min-w-72 bg-[#203f70] px-2 py-1.5 text-center">Chỉ số</th><th className="sticky left-72 z-10 min-w-24 bg-[#203f70] px-2 py-1.5 text-center">Ngưỡng</th>{months.map((month) => <th key={month} className="min-w-20 px-2 py-1.5">{month}</th>)}</tr></thead><tbody>{rows.map((row) => row.type === "group" ? <tr key={row.label} className="bg-[#d9e5f1] font-bold text-[#17375e]"><td className="sticky left-0 z-10 min-w-72 bg-[#d9e5f1] px-2 py-1 text-left">{row.label}</td><td className="sticky left-72 z-10 min-w-24 bg-[#d9e5f1] px-2 py-1" /><td colSpan={12} className="bg-[#d9e5f1] px-2 py-1" /></tr> : <tr key={row.label} className="border-b border-slate-200"><td className="sticky left-0 z-10 bg-white px-2 py-1 text-left whitespace-nowrap">{row.label}</td><td className="sticky left-72 z-10 bg-white px-2 py-1 whitespace-nowrap text-slate-600">{row.thresholdLabel}</td>{row.values.map((value, index) => <td key={`${row.label}-${months[index]}`} className={`px-2 py-1 whitespace-nowrap ${statusClass(row, value)}`}>{formatValue(value, row.unit)}</td>)}</tr>)}</tbody></table></div><p className="mt-4 text-sm text-slate-600">Ngưỡng tham khảo cho bán lẻ thời trang – mỹ phẩm, đặt để minh họa.</p></section>;
}

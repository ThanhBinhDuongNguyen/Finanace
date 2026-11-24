"use client";

import { Bar, BarChart, CartesianGrid, Legend, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";

type GroupRow = { group: string; values: string[]; total?: boolean; aged?: number };

const headers = ["Nhóm hàng", "Doanh thu", "Tỷ trọng DT", "Giá vốn", "Lãi gộp", "Biên gộp", "Tỷ trọng lãi gộp", "Số lượng", "Giá bán TB (nghìn đ)", "Tồn cuối kỳ", "Ngày tồn kho", "Tồn > 90 ngày", "% tồn > 90 ngày"];

const currentRows: GroupRow[] = [
  { group: "Thời trang nữ", aged: 12, values: ["5.297,5", "34,9%", "2.748,4", "2.549,1", "48,1%", "36,4%", "12.431", "426", "6.746", "74", "809,5", "12,0%"] },
  { group: "Thời trang nam", aged: 15, values: ["3.617,2", "23,8%", "1.997,1", "1.620,1", "44,8%", "23,1%", "8.922", "405", "5.580,4", "84", "837,1", "15,0%"] },
  { group: "Giày dép", aged: 33, values: ["2.329,5", "15,3%", "1.343,4", "986,1", "42,3%", "14,1%", "5.893", "395", "5.491", "123", "1.812", "33,0%"] },
  { group: "Túi & phụ kiện", aged: 10, values: ["2.098,5", "13,8%", "950,3", "1.148,2", "54,7%", "16,4%", "5.423", "387", "2.220,2", "70", "222", "10,0%"] },
  { group: "Mỹ phẩm", aged: 5, values: ["1.851,5", "12,2%", "1.156,7", "694,8", "37,5%", "9,9%", "4.514", "410", "2.038", "53", "101,9", "5,0%"] },
  { group: "Tổng cộng", total: true, aged: 17.1, values: ["15.194,2", "100,0%", "8.195,9", "6.998,3", "46,1%", "100,0%", "37.183", "409", "22.075,6", "81", "3.782,5", "17,1%"] },
];

const accumulatedRows: GroupRow[] = [
  { group: "Thời trang nữ", aged: 12, values: ["44.019,1", "34,3%", "23.073,7", "20.947,4", "47,7%", "35,6%", "108.592", "405", "6.746", "74", "809,5", "12,0%"] },
  { group: "Thời trang nam", aged: 15, values: ["30.619,1", "23,9%", "16.879,6", "13.739,5", "44,9%", "23,3%", "76.589", "400", "5.580,4", "84", "837,1", "15,0%"] },
  { group: "Giày dép", aged: 33, values: ["20.313,3", "15,8%", "11.813,2", "8.500,1", "41,8%", "14,4%", "51.681", "393", "5.491", "123", "1.812", "33,0%"] },
  { group: "Túi & phụ kiện", aged: 10, values: ["18.075,6", "14,1%", "8.202,8", "9.872,8", "54,6%", "16,8%", "45.517", "397", "2.220,2", "70", "222", "10,0%"] },
  { group: "Mỹ phẩm", aged: 5, values: ["15.287", "11,9%", "9.497,3", "5.789,7", "37,9%", "9,8%", "38.576", "396", "2.038", "53", "101,9", "5,0%"] },
  { group: "Tổng cộng", total: true, aged: 17.1, values: ["128.306,1", "100,0%", "69.406,6", "58.899,5", "45,9%", "100,0%", "320.915", "400", "22.075,6", "81", "3.782,5", "17,1%"] },
];

const monthlyInventory = [
  { month: "01/2026", women: 5000, men: 4000, shoes: 3500, accessories: 1700, beauty: 1600 }, { month: "02/2026", women: 3700, men: 3000, shoes: 2900, accessories: 1200, beauty: 900 },
  { month: "03/2026", women: 3900, men: 3300, shoes: 3000, accessories: 1200, beauty: 1100 }, { month: "04/2026", women: 4000, men: 3500, shoes: 3000, accessories: 1300, beauty: 900 },
  { month: "05/2026", women: 4200, men: 3600, shoes: 2700, accessories: 1400, beauty: 1100 }, { month: "06/2026", women: 4200, men: 3500, shoes: 2800, accessories: 1500, beauty: 1200 },
  { month: "07/2026", women: 4500, men: 3600, shoes: 3800, accessories: 1400, beauty: 1200 }, { month: "08/2026", women: 4700, men: 3800, shoes: 4200, accessories: 1500, beauty: 1300 },
  { month: "09/2026", women: 4500, men: 3700, shoes: 3700, accessories: 1600, beauty: 1400 }, { month: "10/2026", women: 5100, men: 4400, shoes: 4000, accessories: 1600, beauty: 1800 },
  { month: "11/2026", women: 5900, men: 4200, shoes: 4500, accessories: 1700, beauty: 1800 }, { month: "12/2026", women: 6746, men: 5580.4, shoes: 5491, accessories: 2220.2, beauty: 2038 },
];

function InventoryTable({ title, rows }: { title: string; rows: GroupRow[] }) {
  return <section className="overflow-hidden rounded-xl border border-slate-200 bg-white p-4 shadow-sm"><h2 className="mb-3 text-base font-bold text-[#17375e]">{title}</h2><div className="overflow-x-auto"><table className="min-w-[1400px] w-full border-collapse text-right text-sm"><thead className="bg-[#203f70] text-white"><tr>{headers.map((header, index) => <th key={header} className={`px-2 py-1.5 font-semibold ${index === 0 ? "text-center" : ""}`}>{header}</th>)}</tr></thead><tbody>{rows.map((row) => <tr key={row.group} className={`border-b border-slate-200 ${row.total ? "font-bold text-slate-950" : "text-slate-700"}`}><td className="px-2 py-1 text-left whitespace-nowrap">{row.group}</td>{row.values.map((value, index) => <td key={`${row.group}-${headers[index + 1]}`} className={`px-2 py-1 whitespace-nowrap ${index === row.values.length - 1 && (row.aged ?? 0) > 15 ? "text-red-600" : ""}`}>{value}</td>)}</tr>)}</tbody></table></div></section>;
}

export function InventoryGroupReport() {
  return <div className="space-y-5"><InventoryTable title="Kỳ 12/2026" rows={currentRows} /><InventoryTable title="Lũy kế đến 12/2026 (tồn kho vẫn lấy cuối kỳ)" rows={accumulatedRows} /><section className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5"><h2 className="mb-3 text-base font-bold text-[#17375e]">Tồn kho cuối tháng theo nhóm hàng</h2><div className="h-96"><ResponsiveContainer width="100%" height="100%"><BarChart data={monthlyInventory} margin={{ top: 15, right: 10, left: -10, bottom: 25 }}><CartesianGrid stroke="#d7dde5" vertical={false} /><XAxis dataKey="month" angle={-45} textAnchor="end" interval={0} tick={{ fontSize: 11 }} /><YAxis /><Tooltip /><Legend verticalAlign="top" height={30} /><Bar dataKey="women" name="Thời trang nữ" stackId="inventory" fill="#203f70" /><Bar dataKey="men" name="Thời trang nam" stackId="inventory" fill="#347cba" /><Bar dataKey="shoes" name="Giày dép" stackId="inventory" fill="#0aa3a1" /><Bar dataKey="accessories" name="Túi & phụ kiện" stackId="inventory" fill="#ee7d19" /><Bar dataKey="beauty" name="Mỹ phẩm" stackId="inventory" fill="#8d43b4" /></BarChart></ResponsiveContainer></div></section></div>;
}

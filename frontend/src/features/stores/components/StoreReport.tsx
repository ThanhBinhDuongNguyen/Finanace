"use client";

import { ReportFrame } from "../../../components/ui/ReportFrame";


import { Bar, BarChart, CartesianGrid, ComposedChart, Legend, Line, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";

type StoreRow = { store: string; values: string[]; total?: boolean };

const headers = ["Cửa hàng", "Doanh thu", "Lãi gộp", "Biên gộp", "CP vận hành", "EBITDA CH", "Khấu hao", "LN cửa hàng", "Biên LN", "Footfall", "Số bill", "Chuyển đổi", "Bill TB (nghìn đ)", "SP/bill", "DT/m²/tháng"];

const currentRows: StoreRow[] = [
  { store: "CH01 – Q1 – Nguyễn Trãi", values: ["3.498", "1.645", "47,0%", "548,1", "1.096,9", "45", "1.051,9", "30,1%", "30.559", "5.238", "17,1%", "668", "1,67", "19,43"] },
  { store: "CH02 – Q7 – Crescent Mall", values: ["2.693,1", "1.269,9", "47,2%", "539,9", "730", "45", "685", "25,4%", "17.615", "3.879", "22,0%", "694", "1,64", "17,95"] },
  { store: "CH03 – Thủ Đức – Vincom", values: ["2.742,8", "1.255,1", "45,8%", "488,9", "766,2", "45", "721,2", "26,3%", "19.748", "3.938", "19,9%", "696", "1,61", "17,14"] },
  { store: "CH04 – Hà Nội – Phố Huế", values: ["2.239,6", "1.024,4", "45,7%", "468,4", "556,0", "45", "510,6", "22,8%", "13.637", "3.209", "23,5%", "698", "1,62", "16,00"] },
  { store: "CH05 – Đà Nẵng – Lê Duẩn", values: ["1.203,1", "556,7", "46,3%", "312,4", "244,3", "45", "199,3", "16,6%", "17.484", "2.116", "12,1%", "569", "1,70", "9,25"] },
  { store: "Tổng cộng", total: true, values: ["12.376,6", "5.750,7", "46,5%", "2.357,7", "3.393", "225", "3.168", "25,6%", "99.043", "18.380", "18,6%", "673", "1,64", "16,28"] },
];

const accumulatedRows: StoreRow[] = [
  { store: "CH01 – Q1 – Nguyễn Trãi", values: ["30.229,2", "14.049,2", "46,5%", "5.995,2", "8.054", "540", "7.514", "24,9%", "232.699", "46.326", "19,9%", "653", "1,66", "13,99"] },
  { store: "CH02 – Q7 – Crescent Mall", values: ["25.026,6", "11.632,5", "46,5%", "6.208,1", "5.424,4", "540", "4.884,4", "19,5%", "173.229", "37.751", "21,8%", "663", "1,60", "13,90"] },
  { store: "CH03 – Thủ Đức – Vincom", values: ["21.381,6", "9.855,2", "46,1%", "5.181,3", "4.673,9", "540", "4.133,9", "19,3%", "155.931", "31.522", "20,2%", "678", "1,64", "11,14"] },
  { store: "CH04 – Hà Nội – Phố Huế", values: ["19.699,7", "9.055,7", "46,0%", "5.219,5", "3.836,2", "540", "3.296,2", "16,7%", "145.744", "28.639", "19,7%", "688", "1,65", "11,73"] },
  { store: "CH05 – Đà Nẵng – Lê Duẩn", values: ["9.815,6", "4.540,5", "46,3%", "3.512,4", "1.028,1", "540", "488,1", "5,0%", "137.144", "18.337", "13,4%", "535", "1,65", "6,29"] },
  { store: "Tổng cộng", total: true, values: ["106.152,7", "49.133,1", "46,3%", "26.116,5", "23.016,6", "2.700", "20.316,6", "19,1%", "844.747", "162.575", "19,2%", "653", "1,64", "11,64"] },
];

const monthlyTraffic = [
  { month: "01/2026", footfall: 78_000, conversion: 19 }, { month: "02/2026", footfall: 50_000, conversion: 21 }, { month: "03/2026", footfall: 60_000, conversion: 18.7 },
  { month: "04/2026", footfall: 68_000, conversion: 17.8 }, { month: "05/2026", footfall: 65_000, conversion: 19 }, { month: "06/2026", footfall: 62_000, conversion: 20.3 },
  { month: "07/2026", footfall: 68_000, conversion: 17.6 }, { month: "08/2026", footfall: 68_000, conversion: 19.4 }, { month: "09/2026", footfall: 69_000, conversion: 18.5 },
  { month: "10/2026", footfall: 75_000, conversion: 18 }, { month: "11/2026", footfall: 77_000, conversion: 19.3 }, { month: "12/2026", footfall: 99_043, conversion: 18.6 },
];

const averageBill = [{ code: "CH01", value: 668 }, { code: "CH02", value: 694 }, { code: "CH03", value: 696 }, { code: "CH04", value: 698 }, { code: "CH05", value: 569 }];

function StoreTable({ title, rows }: { title: string; rows: StoreRow[] }) {
  return <ReportFrame title={title} description="Đơn vị tài chính: triệu đồng; tỷ lệ và số lượng theo tiêu đề cột." scrollHint><div className="finance-table-scroll" tabIndex={0} role="region" aria-label="Bảng số liệu có thể cuộn ngang"><table className="finance-table min-w-[1450px]"><caption className="sr-only">{title}</caption><thead className="bg-[#203f70] text-white"><tr>{headers.map((header, index) => <th scope="col" key={header} className={`px-2 py-1.5 font-semibold ${index === 0 ? "text-center" : ""}`}>{header}</th>)}</tr></thead><tbody>{rows.map((row) => <tr key={row.store} className={`border-b border-slate-200 ${row.total ? "finance-row-total font-bold text-slate-950" : "text-slate-700"}`}><td className="px-2 py-1 text-left whitespace-nowrap">{row.store}</td>{row.values.map((value, index) => <td key={`${row.store}-${headers[index + 1]}`} className="px-2 py-1 whitespace-nowrap">{value}</td>)}</tr>)}</tbody></table></div></ReportFrame>;
}

export function StoreReport() {
  return <div className="space-y-5"><StoreTable title="Kỳ 12/2026" rows={currentRows} /><StoreTable title="Lũy kế đến 12/2026" rows={accumulatedRows} /><div className="grid gap-5 2xl:grid-cols-2"><ReportFrame title="Footfall & tỷ lệ chuyển đổi từng tháng (toàn chuỗi)"><div className="finance-chart"><ResponsiveContainer width="100%" height="100%"><ComposedChart data={monthlyTraffic} margin={{ top: 15, right: 10, left: -10, bottom: 25 }}><CartesianGrid stroke="#d7dde5" vertical={false} /><XAxis dataKey="month" angle={-45} textAnchor="end" interval={0} tick={{ fontSize: 11 }} /><YAxis yAxisId="left" /><YAxis yAxisId="right" orientation="right" unit="%" /><Tooltip /><Legend verticalAlign="top" height={30} /><Bar isAnimationActive={false} yAxisId="left" dataKey="footfall" name="Footfall" fill="#347cba" /><Line isAnimationActive={false} yAxisId="right" dataKey="conversion" name="Chuyển đổi %" type="monotone" stroke="#ee7d19" strokeWidth={3} dot={{ r: 3 }} /></ComposedChart></ResponsiveContainer></div></ReportFrame><ReportFrame title="Bill trung bình theo cửa hàng (nghìn đ, kỳ)"><div className="finance-chart"><ResponsiveContainer width="100%" height="100%"><BarChart data={averageBill} margin={{ top: 15, right: 10, left: -10, bottom: 15 }}><CartesianGrid stroke="#d7dde5" vertical={false} /><XAxis dataKey="code" /><YAxis domain={[0, 700]} /><Tooltip /><Bar isAnimationActive={false} dataKey="value" name="Bill TB" fill="#203f70" /></BarChart></ResponsiveContainer></div></ReportFrame></div></div>;
}

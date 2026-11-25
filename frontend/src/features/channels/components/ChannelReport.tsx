"use client";

import { ReportFrame } from "../../../components/ui/ReportFrame";


import { Bar, BarChart, CartesianGrid, Legend, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";

type ChannelRow = { channel: string; values: string[]; total?: boolean };

const headers = ["Kênh", "Doanh thu", "Tỷ trọng", "Lãi gộp", "Biên gộp", "CP trực tiếp", "LN đóng góp", "Biên đóng góp", "Lượt khách/truy cập", "Số bill", "Chuyển đổi", "Bill TB (nghìn đ)"];

const currentRows: ChannelRow[] = [
  { channel: "Cửa hàng", values: ["12.376,6", "81,5%", "5.750,7", "46,5%", "2.357,7", "3.393", "27,4%", "99.043", "18.380", "18,6%", "673"] },
  { channel: "Website", values: ["646,8", "4,3%", "288,3", "44,6%", "199,9", "88,4", "13,7%", "35.282", "1.235", "3,5%", "524"] },
  { channel: "Shopee", values: ["1.124,4", "7,4%", "494,3", "44,0%", "379,1", "115,2", "10,2%", "91.005", "1.820", "2,0%", "618"] },
  { channel: "TikTok Shop", values: ["1.046,4", "6,9%", "465", "44,4%", "364,2", "100,8", "9,6%", "122.029", "2.197", "1,8%", "476"] },
  { channel: "Tổng cộng", total: true, values: ["15.194,2", "100,0%", "6.998,3", "46,1%", "3.300,9", "3.697,4", "24,3%", "347.359", "23.632", "6,8%", "643"] },
];

const accumulatedRows: ChannelRow[] = [
  { channel: "Cửa hàng", values: ["106.152,7", "82,7%", "49.133,1", "46,3%", "26.116,5", "23.016,6", "21,7%", "844.747", "162.575", "19,2%", "653"] },
  { channel: "Website", values: ["5.688,7", "4,4%", "2.515,9", "44,2%", "1.943", "572,9", "10,1%", "306.132", "10.711", "3,5%", "531"] },
  { channel: "Shopee", values: ["9.275,7", "7,2%", "4.075,9", "43,9%", "3.328,2", "747,7", "8,1%", "753.881", "15.079", "2,0%", "615"] },
  { channel: "TikTok Shop", values: ["7.189", "5,6%", "3.174,6", "44,2%", "2.806,3", "368,3", "5,1%", "820.614", "14.772", "1,8%", "487"] },
  { channel: "Tổng cộng", total: true, values: ["128.306,1", "100,0%", "58.899,5", "45,9%", "34.194", "24.705,5", "19,3%", "2.725.374", "203.137", "7,5%", "632"] },
];

const revenueByChannel = [
  { month: "01/2026", store: 9_800, website: 400, shopee: 800, tiktok: 555.9 }, { month: "02/2026", store: 7_000, website: 350, shopee: 500, tiktok: 406.8 },
  { month: "03/2026", store: 7_600, website: 400, shopee: 700, tiktok: 528.9 }, { month: "04/2026", store: 8_000, website: 410, shopee: 600, tiktok: 515.8 },
  { month: "05/2026", store: 8_150, website: 420, shopee: 740, tiktok: 559.1 }, { month: "06/2026", store: 8_100, website: 400, shopee: 650, tiktok: 470.6 },
  { month: "07/2026", store: 8_200, website: 470, shopee: 780, tiktok: 567 }, { month: "08/2026", store: 8_600, website: 450, shopee: 900, tiktok: 613.2 },
  { month: "09/2026", store: 8_500, website: 430, shopee: 900, tiktok: 706.8 }, { month: "10/2026", store: 9_200, website: 520, shopee: 900, tiktok: 841.5 },
  { month: "11/2026", store: 10_100, website: 550, shopee: 900, tiktok: 926.3 }, { month: "12/2026", store: 12_376.6, website: 646.8, shopee: 1_124.4, tiktok: 1_046.4 },
];

function ChannelTable({ title, rows }: { title: string; rows: ChannelRow[] }) {
  return <ReportFrame title={title} description="Đơn vị tài chính: triệu đồng; tỷ lệ và số lượng theo tiêu đề cột." scrollHint><div className="finance-table-scroll" tabIndex={0} role="region" aria-label="Bảng số liệu có thể cuộn ngang"><table className="finance-table min-w-[1260px]"><caption className="sr-only">{title}</caption><thead className="bg-[#203f70] text-white"><tr>{headers.map((header, index) => <th scope="col" key={header} className={`px-2 py-1.5 font-semibold ${index === 0 ? "text-center" : ""}`}>{header}</th>)}</tr></thead><tbody>{rows.map((row) => <tr key={row.channel} className={`border-b border-slate-200 ${row.total ? "finance-row-total font-bold text-slate-950" : "text-slate-700"}`}><td className="px-2 py-1 text-left whitespace-nowrap">{row.channel}</td>{row.values.map((value, index) => <td key={`${row.channel}-${headers[index + 1]}`} className="px-2 py-1 whitespace-nowrap">{value}</td>)}</tr>)}</tbody></table></div></ReportFrame>;
}

export function ChannelReport() {
  return <div className="space-y-5"><ChannelTable title="Kỳ 12/2026" rows={currentRows} /><ChannelTable title="Lũy kế đến 12/2026" rows={accumulatedRows} /><ReportFrame title="Doanh thu theo kênh từng tháng"><div className="finance-chart"><ResponsiveContainer width="100%" height="100%"><BarChart data={revenueByChannel} margin={{ top: 15, right: 10, left: -10, bottom: 25 }}><CartesianGrid stroke="#d7dde5" vertical={false} /><XAxis dataKey="month" angle={-45} textAnchor="end" interval={0} tick={{ fontSize: 11 }} /><YAxis /><Tooltip /><Legend verticalAlign="top" height={30} /><Bar isAnimationActive={false} dataKey="store" name="Cửa hàng" stackId="revenue" fill="#203f70" /><Bar isAnimationActive={false} dataKey="website" name="Website" stackId="revenue" fill="#347cba" /><Bar isAnimationActive={false} dataKey="shopee" name="Shopee" stackId="revenue" fill="#0aa3a1" /><Bar isAnimationActive={false} dataKey="tiktok" name="TikTok Shop" stackId="revenue" fill="#ee7d19" /></BarChart></ResponsiveContainer></div></ReportFrame></div>;
}

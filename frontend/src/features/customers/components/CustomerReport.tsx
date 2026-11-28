"use client";

import { ReportFrame } from "../../../components/ui/ReportFrame";


import { Bar, BarChart, CartesianGrid, Cell, Legend, Pie, PieChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";

type CustomerRow = { segment: string; values: string[]; total?: boolean };

const headers = ["Phân khúc", "Số khách", "Tỷ trọng khách", "Doanh thu", "Tỷ trọng DT", "Số bill", "Bill TB (nghìn đ)", "Tần suất mua", "Khách mới", "% DT từ khách quay lại"];

const ageCurrent: CustomerRow[] = [
  { segment: "18–24", values: ["2.661", "22,2%", "1.803,3", "16,3%", "3.592", "502", "1,35", "1.208", "70,1%"] },
  { segment: "25–34", values: ["4.553", "38,0%", "4.168,1", "37,7%", "6.201", "672", "1,36", "1.973", "71,8%"] },
  { segment: "35–44", values: ["3.037", "25,4%", "3.338,8", "30,2%", "4.137", "807", "1,36", "1.311", "71,9%"] },
  { segment: "45+", values: ["1.724", "14,4%", "1.741,4", "15,8%", "2.352", "740", "1,36", "738", "72,2%"] },
  { segment: "Tổng cộng", total: true, values: ["11.975", "100,0%", "11.051,6", "100,0%", "16.282", "679", "1,36", "5.230", "71,6%"] },
];

const ageAccumulated: CustomerRow[] = [
  { segment: "18–24", values: ["23.395", "22,1%", "15.292,3", "16,4%", "31.211", "490", "1,33", "11.303", "67,6%"] },
  { segment: "25–34", values: ["40.230", "38,1%", "35.259,8", "37,8%", "53.881", "654", "1,34", "19.077", "68,4%"] },
  { segment: "35–44", values: ["26.265", "24,9%", "27.583,6", "29,6%", "35.136", "785", "1,34", "12.516", "68,2%"] },
  { segment: "45+", values: ["15.735", "14,9%", "15.207,1", "16,3%", "21.103", "721", "1,34", "7.405", "68,7%"] },
  { segment: "Tổng cộng", total: true, values: ["105.625", "100,0%", "93.342,8", "100,0%", "141.331", "660", "1,34", "50.297", "68,2%"] },
];

const genderCurrent: CustomerRow[] = [
  { segment: "Nữ", values: ["8.166", "68,2%", "7.520,4", "68,0%", "11.070", "679", "1,36", "3.628", "71,0%"] },
  { segment: "Nam", values: ["3.809", "31,8%", "3.531,2", "32,0%", "5.212", "678", "1,37", "1.602", "72,9%"] },
  { segment: "Tổng cộng", total: true, values: ["11.975", "100,0%", "11.051,6", "100,0%", "16.282", "679", "1,36", "5.230", "71,6%"] },
];

const revenueByAge = [
  { name: "18–24", value: 1803.3, color: "var(--brand)" }, { name: "25–34", value: 4168.1, color: "var(--brand)" },
  { name: "35–44", value: 3338.8, color: "var(--chart-2)" }, { name: "45+", value: 1741.4, color: "var(--chart-3)" },
];

const monthlyCustomers = [
  { month: "01/2026", newCustomers: 5300, returningCustomers: 4800 }, { month: "02/2026", newCustomers: 3600, returningCustomers: 3300 },
  { month: "03/2026", newCustomers: 3700, returningCustomers: 3600 }, { month: "04/2026", newCustomers: 3900, returningCustomers: 4000 },
  { month: "05/2026", newCustomers: 4200, returningCustomers: 4300 }, { month: "06/2026", newCustomers: 4000, returningCustomers: 4100 },
  { month: "07/2026", newCustomers: 3600, returningCustomers: 4100 }, { month: "08/2026", newCustomers: 4200, returningCustomers: 4700 },
  { month: "09/2026", newCustomers: 4000, returningCustomers: 4500 }, { month: "10/2026", newCustomers: 4000, returningCustomers: 5100 },
  { month: "11/2026", newCustomers: 4500, returningCustomers: 5800 }, { month: "12/2026", newCustomers: 5230, returningCustomers: 6700 },
];

function CustomerTable({ title, rows }: { title: string; rows: CustomerRow[] }) {
  return <ReportFrame title={title} description="Đơn vị tài chính: triệu đồng; tỷ lệ và số lượng theo tiêu đề cột." scrollHint><div className="finance-table-scroll" tabIndex={0} role="region" aria-label="Bảng số liệu có thể cuộn ngang"><table className="finance-table min-w-[1140px]"><caption className="sr-only">{title}</caption><thead className="bg-[var(--brand)] text-white"><tr>{headers.map((header, index) => <th scope="col" key={header} className={`px-2 py-1.5 font-semibold ${index === 0 ? "text-center" : ""}`}>{header}</th>)}</tr></thead><tbody>{rows.map((row) => <tr key={row.segment} className={`border-b border-slate-200 ${row.total ? "finance-row-total font-bold text-slate-950" : "text-slate-700"}`}><td className="px-2 py-1 text-left whitespace-nowrap">{row.segment}</td>{row.values.map((value, index) => <td key={`${row.segment}-${headers[index + 1]}`} className="px-2 py-1 whitespace-nowrap">{value}</td>)}</tr>)}</tbody></table></div></ReportFrame>;
}

export function CustomerReport() {
  return <div className="space-y-5"><CustomerTable title="Theo độ tuổi – kỳ 12/2026" rows={ageCurrent} /><CustomerTable title="Theo độ tuổi – lũy kế" rows={ageAccumulated} /><CustomerTable title="Theo giới tính – kỳ 12/2026" rows={genderCurrent} /><div className="grid gap-5 2xl:grid-cols-2"><ReportFrame title="Doanh thu theo độ tuổi (kỳ)"><div className="finance-chart"><ResponsiveContainer width="100%" height="100%"><PieChart><Pie isAnimationActive={false} data={revenueByAge} dataKey="value" nameKey="name" cx="50%" cy="54%" innerRadius="50%" outerRadius="82%" paddingAngle={1}>{revenueByAge.map((item) => <Cell key={item.name} fill={item.color} />)}</Pie><Tooltip /><Legend verticalAlign="top" height={35} /></PieChart></ResponsiveContainer></div></ReportFrame><ReportFrame title="Khách mới / quay lại từng tháng"><div className="finance-chart"><ResponsiveContainer width="100%" height="100%"><BarChart data={monthlyCustomers} margin={{ top: 15, right: 10, left: -10, bottom: 25 }}><CartesianGrid stroke="var(--line)" vertical={false} /><XAxis dataKey="month" angle={-45} textAnchor="end" interval={0} tick={{ fontSize: 12 }} /><YAxis /><Tooltip /><Legend verticalAlign="top" height={30} /><Bar isAnimationActive={false} dataKey="newCustomers" name="Khách mới" stackId="customers" fill="var(--brand)" /><Bar isAnimationActive={false} dataKey="returningCustomers" name="Khách quay lại" stackId="customers" fill="var(--chart-2)" /></BarChart></ResponsiveContainer></div></ReportFrame></div></div>;
}

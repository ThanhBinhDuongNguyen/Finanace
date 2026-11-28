"use client";

import { Bar, BarChart, CartesianGrid, Cell, ComposedChart, Legend, Line, Pie, PieChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { MetricCard } from "../../../components/ui/MetricCard";
import { ReportFrame } from "../../../components/ui/ReportFrame";
import { OperationsSummary } from "./OperationsSummary";
import { channelMix, grossMargins, metrics, monthlyPerformance, storeProfit } from "../data/dashboard.mock";

export function FinanceDashboard() {
  return <div className="space-y-6">
    <OperationsSummary />
    <section aria-labelledby="illustrative-summary-heading">
      <div className="mb-4 flex flex-wrap items-center justify-between gap-2">
        <div><h2 id="illustrative-summary-heading" className="text-base font-semibold text-slate-900">Báo cáo quản trị bán lẻ</h2><p className="mt-1 text-sm text-slate-500">Kỳ 12/2026 · Đơn vị tài chính: triệu đồng, trừ chỉ số có ghi riêng.</p></div>
        <span className="finance-source-label">Dữ liệu minh họa</span>
      </div>
      <div className="grid grid-cols-2 gap-3 xl:grid-cols-4 2xl:grid-cols-6">{metrics.map((metric) => <MetricCard key={metric.label} {...metric} />)}</div>
    </section>
    <div className="grid gap-5 xl:grid-cols-2 2xl:grid-cols-3">
      <ReportFrame title="Doanh thu & lợi nhuận sau thuế" description="Theo tháng · Đơn vị: triệu đồng">
        <div className="finance-chart"><ResponsiveContainer width="100%" height="100%">
          <ComposedChart data={monthlyPerformance} margin={{ top: 12, right: 10, left: -12, bottom: 38 }}>
            <CartesianGrid stroke="var(--line)" vertical={false} /><XAxis dataKey="month" angle={-50} textAnchor="end" interval={0} tick={{ fontSize: 12 }} />
            <YAxis yAxisId="left" tick={{ fontSize: 12 }} /><YAxis yAxisId="right" orientation="right" tick={{ fontSize: 12 }} />
            <Tooltip /><Legend verticalAlign="top" height={30} /><Bar isAnimationActive={false} yAxisId="left" dataKey="revenue" name="Doanh thu" fill="var(--brand)" />
            <Line isAnimationActive={false} yAxisId="right" type="monotone" dataKey="profit" name="LN sau thuế" stroke="var(--chart-3)" strokeWidth={3} dot={{ r: 3 }} />
          </ComposedChart>
        </ResponsiveContainer></div>
      </ReportFrame>
      <ReportFrame title="Cơ cấu doanh thu theo kênh" description="Tỷ trọng doanh thu kỳ 12/2026">
        <div className="finance-chart"><ResponsiveContainer width="100%" height="100%">
          <PieChart><Pie isAnimationActive={false} data={channelMix} dataKey="value" nameKey="name" cx="50%" cy="55%" innerRadius="45%" outerRadius="75%" paddingAngle={1}>{channelMix.map((item) => <Cell key={item.name} fill={item.color} />)}</Pie><Tooltip formatter={(value) => `${value}%`} /><Legend verticalAlign="top" height={52} /></PieChart>
        </ResponsiveContainer></div>
      </ReportFrame>
      <ReportFrame title="Lợi nhuận theo cửa hàng" description="Kỳ 12/2026 · Đơn vị: triệu đồng">
        <div className="finance-chart"><ResponsiveContainer width="100%" height="100%">
          <BarChart data={storeProfit} layout="vertical" margin={{ top: 28, right: 15, left: 5, bottom: 15 }}><CartesianGrid stroke="var(--line)" horizontal={false} /><XAxis type="number" tick={{ fontSize: 12 }} /><YAxis dataKey="name" type="category" width={145} tick={{ fontSize: 12 }} /><Tooltip /><Bar isAnimationActive={false} dataKey="value" name="Lợi nhuận" fill="var(--brand)" /></BarChart>
        </ResponsiveContainer></div>
      </ReportFrame>
      <ReportFrame title="Biên lãi gộp theo nhóm hàng" description="Kỳ 12/2026 · Đơn vị: %">
        <div className="finance-chart"><ResponsiveContainer width="100%" height="100%">
          <BarChart data={grossMargins} margin={{ top: 10, right: 10, left: -15, bottom: 40 }}><CartesianGrid stroke="var(--line)" vertical={false} /><XAxis dataKey="name" angle={-18} textAnchor="end" interval={0} tick={{ fontSize: 12 }} /><YAxis unit="%" domain={[0, 60]} /><Tooltip formatter={(value) => `${value}%`} /><Bar isAnimationActive={false} dataKey="value" name="Biên lãi gộp" fill="var(--chart-2)" /></BarChart>
        </ResponsiveContainer></div>
      </ReportFrame>
    </div>
  </div>;
}

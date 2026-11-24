"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import type { ReactNode } from "react";
import { useState } from "react";
import { periods } from "../../features/dashboard/data/dashboard.mock";
import { financeNavigation } from "../../routes/navigation";

export function FinanceShell({ children }: { children: ReactNode }) {
  const pathname = usePathname();
  const [sidebarExpanded, setSidebarExpanded] = useState(true);
  const [period, setPeriod] = useState(periods[0]);
  const current = financeNavigation.find((item) => item.href === pathname) ?? financeNavigation[0];

  return <main className="min-h-screen bg-slate-100 text-slate-800 lg:flex"><aside className={`border-b border-slate-700 bg-[#17375e] text-white transition-[width] duration-200 lg:sticky lg:top-0 lg:h-screen lg:shrink-0 lg:border-b-0 lg:border-r ${sidebarExpanded ? "lg:w-56" : "lg:w-[68px]"}`}><div className={`hidden border-b border-white/10 py-5 lg:flex ${sidebarExpanded ? "px-5" : "justify-center px-2"}`}><div className={sidebarExpanded ? "" : "sr-only"}><p className="text-lg font-bold tracking-tight">Nexus Finance</p><p className="mt-1 text-xs text-blue-200">Báo cáo quản trị bán lẻ</p></div>{!sidebarExpanded && <span className="text-lg font-bold">NF</span>}</div><nav className={`flex overflow-x-auto px-3 py-3 lg:block lg:space-y-1 lg:py-5 ${sidebarExpanded ? "lg:px-3" : "lg:px-2"}`} aria-label="Báo cáo tài chính">{financeNavigation.map((item) => <Link key={item.slug} href={item.href} title={item.label} className={`shrink-0 rounded-md px-3 py-2 text-left text-sm font-medium transition lg:block lg:w-full ${sidebarExpanded ? "lg:text-left" : "lg:text-center"} ${pathname === item.href ? "bg-white text-[#17375e] shadow-sm" : "text-blue-100 hover:bg-white/10 hover:text-white"}`}><span className="lg:hidden">{item.label}</span><span className={`hidden lg:inline ${sidebarExpanded ? "" : "text-xs font-bold"}`}>{sidebarExpanded ? item.label : item.label.slice(0, 2).toUpperCase()}</span></Link>)}</nav></aside><section className="min-w-0 flex-1"><header className="flex flex-wrap items-center justify-between gap-3 border-b border-slate-200 bg-white px-5 py-4 sm:px-7"><div className="flex items-center gap-3"><button onClick={() => setSidebarExpanded((value) => !value)} className="hidden rounded-md border border-slate-300 px-2.5 py-1.5 text-lg leading-none text-[#17375e] transition hover:bg-slate-100 lg:block" aria-label={sidebarExpanded ? "Thu gọn sidebar" : "Mở sidebar"} title={sidebarExpanded ? "Thu gọn sidebar" : "Mở sidebar"}>{sidebarExpanded ? "‹" : "›"}</button><div><h1 className="text-lg font-bold text-[#17375e]">{current.title}</h1><p className="mt-0.5 text-xs text-slate-500">Đơn vị: triệu đồng · dữ liệu minh họa</p></div></div><label className="flex items-center gap-2 text-sm text-slate-600">Kỳ<select value={period} onChange={(event) => setPeriod(event.target.value)} className="rounded-md border border-slate-300 bg-white px-3 py-2 font-medium text-slate-800 outline-none focus:border-[#17375e]">{periods.map((item) => <option key={item}>{item}</option>)}</select></label></header><div className="p-5 sm:p-7">{children}</div></section></main>;
}

"use client";

import Link from "next/link";
import { trapDialogFocus } from "../../lib/dialog-focus";
import { usePathname, useSearchParams } from "next/navigation";
import { type ReactNode, useEffect, useRef, useSyncExternalStore } from "react";
import { financeNavigation } from "../../routes/navigation";
import { AppIcon, type IconName } from "../ui/AppIcon";

const icons: IconName[] = ["overview", "profit", "balance", "cash", "health", "channel", "store", "tracking", "inventory", "customers", "operations"];
const operationLinks = [
  ["imports", "Nhập dữ liệu"], ["reconciliation", "Đối soát"], ["exceptions", "Ngoại lệ"], ["ledger", "Sổ quản trị"],
  ["classification", "Phân loại"], ["report", "Báo cáo nguồn"], ["extensions", "Đơn & chứng từ"], ["controls", "Kiểm soát"],
] as const;
const storageKey = "finance.sidebar.collapsed";
function subscribe(callback: () => void) { window.addEventListener("storage", callback); window.addEventListener("finance-sidebar", callback); return () => { window.removeEventListener("storage", callback); window.removeEventListener("finance-sidebar", callback); }; }
function getCollapsed() { try { return localStorage.getItem(storageKey) === "true"; } catch { return false; } }
function toggleSidebar() { try { localStorage.setItem(storageKey, String(!getCollapsed())); } catch { /* Private browsing can disable persistent preferences. */ } window.dispatchEvent(new Event("finance-sidebar")); }

export function FinanceShell({ children }: { children: ReactNode }) {
  const pathname = usePathname();
  const search = useSearchParams();
  const collapsed = useSyncExternalStore(subscribe, getCollapsed, () => false);
  const drawer = useRef<HTMLDialogElement>(null);
  const operationPage = pathname === "/van-hanh-finance";
  const selectedSection = search?.get("section") ?? "imports";
  const current = financeNavigation.find(item => item.href === pathname) ?? financeNavigation[0];
  useEffect(() => { drawer.current?.close(); }, [pathname, selectedSection]);

  const navigation = (mobile = false) => <nav aria-label={mobile ? "Điều hướng di động" : "Điều hướng chính"} className="finance-navigation">
    {[{ label: "Tài chính", start: 0, end: 5 }, { label: "Phân tích bán lẻ", start: 5, end: 10 }, { label: "Vận hành", start: 10, end: 11 }].map(group => <div className="finance-nav-group" key={group.label}>
      <p className={collapsed && !mobile ? "sr-only" : "finance-nav-label"}>{group.label}</p>
      {financeNavigation.slice(group.start, group.end).map((item, index) => <div key={item.slug}>
        <Link href={item.href} aria-current={pathname === item.href ? "page" : undefined} className={`finance-nav-link ${pathname === item.href ? "is-active" : ""} ${collapsed && !mobile ? "is-icon-only" : ""}`} aria-label={item.label} title={collapsed && !mobile ? item.label : undefined}>
          <AppIcon name={icons[group.start + index]} />
          <span className={collapsed && !mobile ? "sr-only" : ""}>{item.label}</span>
          {collapsed && !mobile && <span className="finance-nav-tooltip" aria-hidden="true">{item.label}</span>}
        </Link>
        {item.slug === "van-hanh-finance" && operationPage && (!collapsed || mobile) && <div className="finance-subnav">{operationLinks.map(([id, label]) => <Link key={id} href={`/van-hanh-finance?section=${id}`} onClick={event => { if (!event.metaKey && !event.ctrlKey && !event.shiftKey && !event.altKey && event.button === 0) { event.preventDefault(); window.history.pushState(null, "", `/van-hanh-finance?section=${id}`); } }} aria-current={selectedSection === id ? "location" : undefined} className={selectedSection === id ? "is-active" : ""}>{label}</Link>)}</div>}
      </div>)}
    </div>)}
  </nav>;

  return <div className={`finance-shell ${collapsed ? "is-collapsed" : ""}`}>
    <a href="#main-content" className="skip-link">Đến nội dung chính</a>
    <aside className="finance-sidebar">
      <Link href="/" className="finance-brand" aria-label="Nexus Finance — Tổng quan"><span className="nexus-brand-mark">N</span><span className={collapsed ? "sr-only" : "finance-brand-text"}>NexusOMS<small>Tài chính</small></span></Link>
      <div className="finance-sidebar-scroll">{navigation()}</div>
      <div className="finance-sidebar-footer"><button type="button" onClick={toggleSidebar} aria-label={collapsed ? "Mở sidebar" : "Thu gọn sidebar"} aria-expanded={!collapsed} className="finance-collapse-button"><AppIcon name="chevron" className={collapsed ? "rotate-180" : ""} /><span className={collapsed ? "sr-only" : ""}>Thu gọn menu</span></button></div>
    </aside>
    <div className="finance-main-column">
      <header className="finance-topbar">
        <div className="flex min-w-0 items-center gap-3"><button type="button" className="finance-mobile-menu" aria-label="Mở menu điều hướng" onClick={() => drawer.current?.showModal()}><AppIcon name="menu" /></button><p className="truncate text-sm text-slate-500">Không gian làm việc <span className="mx-2 text-slate-300">/</span><span className="font-medium text-slate-700">{operationPage ? "Vận hành" : "Báo cáo"}</span></p></div>
        <Link className="finance-topbar-action" href={operationPage ? "/" : "/van-hanh-finance"}>{operationPage ? "Xem tổng quan" : "Vận hành Finance"}<AppIcon name="arrow" width="16" height="16" /></Link>
      </header>
      <main id="main-content" tabIndex={-1} className="finance-main-content">
        <div className="finance-page-heading"><div><p className="finance-eyebrow">{operationPage ? "Nghiệp vụ và kiểm soát" : "Phân tích tài chính"}</p><h1>{current.title}</h1><p className="finance-page-description">{operationPage ? "Xử lý chứng từ, đối soát và phê duyệt theo quyền được cấp." : "Theo dõi kết quả và các chỉ số quản trị chuỗi bán lẻ."}</p></div><div className="finance-page-context">{operationPage ? <span className="finance-status" data-tone="info">Dữ liệu từ hệ thống</span> : <><span className="finance-status" data-tone="warning">Báo cáo minh họa</span><span className="text-xs text-slate-500">Năm 2026 · Triệu đồng</span></>}</div></div>
        {children}
        <footer className="finance-page-footer">Nexus Finance<span>{operationPage ? "Quyền thao tác được kiểm tra tại hệ thống." : "Các chỉ số vận hành API được ghi nguồn riêng tại từng khu vực."}</span></footer>
      </main>
    </div>
    <dialog ref={drawer} className="finance-mobile-drawer" aria-label="Menu điều hướng" onClick={event => { if (event.target === drawer.current) drawer.current.close(); }} onKeyDown={trapDialogFocus}>
      <div className="flex items-center justify-between border-b border-white/10 px-5 py-4"><span className="font-semibold">NexusOMS · Tài chính</span><button type="button" aria-label="Đóng menu điều hướng" onClick={() => drawer.current?.close()} className="rounded p-2 hover:bg-white/10"><AppIcon name="close" /></button></div>
      {navigation(true)}
    </dialog>
  </div>;
}

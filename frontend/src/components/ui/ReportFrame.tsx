import type { ReactNode } from "react";

type ReportFrameProps = { title: string; children: ReactNode; description?: string; className?: string; scrollHint?: boolean };

export function ReportFrame({ title, children, description, className = "", scrollHint = false }: ReportFrameProps) {
  return <section className={`finance-report ${className}`} aria-label={title}>
    <header className="finance-report-heading">
      <div><h2 className="finance-report-title">{title}</h2>{description && <p className="finance-report-meta">{description}</p>}</div>
      <span className="finance-source-label">Minh họa</span>
    </header>
    {children}
    {scrollHint && <p className="finance-report-hint">Cuộn ngang để xem đủ các cột. Cột chỉ tiêu được giữ cố định để đối chiếu.</p>}
  </section>;
}

export function ReportTableFrame({ title, children, description = "Đơn vị: triệu đồng, trừ các cột có ghi đơn vị riêng.", className = "" }: ReportFrameProps) {
  return <ReportFrame title={title} description={description} className={className}>
    <div className="finance-table-scroll" tabIndex={0} role="region" aria-label={`Bảng ${title}. Có thể cuộn ngang để xem các cột.`}>
      {children}
    </div>
    <p className="finance-report-hint">Cuộn ngang để xem đủ các cột. Cột chỉ tiêu được giữ cố định để đối chiếu.</p>
  </ReportFrame>;
}

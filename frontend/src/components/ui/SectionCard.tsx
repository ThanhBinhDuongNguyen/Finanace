import type { ReactNode } from "react";

export function SectionCard({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="finance-panel">
      <h2 className="finance-report-title mb-5">{title}</h2>
      {children}
    </section>
  );
}

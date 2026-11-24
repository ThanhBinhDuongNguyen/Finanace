type MetricCardProps = {
  label: string;
  value: string;
  tone?: "default" | "positive" | "warning";
};

const toneClass = { default: "text-slate-900", positive: "text-emerald-700", warning: "text-amber-700" };

export function MetricCard({ label, value, tone = "default" }: MetricCardProps) {
  return (
    <article className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
      <p className="text-xs font-medium uppercase tracking-wide text-slate-500">{label}</p>
      <p className={`mt-2 text-2xl font-bold tracking-tight ${toneClass[tone]}`}>{value}</p>
    </article>
  );
}

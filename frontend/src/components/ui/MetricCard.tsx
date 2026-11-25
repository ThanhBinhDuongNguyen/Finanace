type MetricCardProps = {
  label: string;
  value: string;
  tone?: "default" | "positive" | "warning";
};

const toneClass = { default: "text-slate-900", positive: "text-emerald-700", warning: "text-amber-700" };

export function MetricCard({ label, value, tone = "default" }: MetricCardProps) {
  return (
    <article className="min-w-0 rounded-xl border border-slate-200 bg-white px-4 py-4">
      <p className="text-sm font-medium leading-5 text-slate-500">{label}</p>
      <p className={`mt-2 text-[1.65rem] font-semibold leading-tight tracking-tight tabular-nums ${toneClass[tone]}`}>{value}</p>
    </article>
  );
}

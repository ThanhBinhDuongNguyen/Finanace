import type { SVGProps } from "react";

export type IconName = "overview" | "profit" | "balance" | "cash" | "health" | "channel" | "store" | "tracking" | "inventory" | "customers" | "operations" | "menu" | "chevron" | "close" | "arrow";
const paths: Record<IconName, string> = {
  overview: "M3 3h7v7H3z M14 3h7v7h-7z M3 14h7v7H3z M14 14h7v7h-7z",
  profit: "M4 20V5 M4 20h17 M8 16l4-5 4 2 5-8",
  balance: "M4 4h16v16H4z M4 9h16 M11 9v11 M7 13h1 M7 17h1 M14 13h3 M14 17h3",
  cash: "M4 7h15l-4-4 M20 17H5l4 4 M20 7v4 M4 17v-4",
  health: "M3 12h4l3-7 4 14 3-7h4",
  channel: "M12 3v6 M5 15v6 M19 15v6 M5 15V9h14v6 M2 15h6 M16 15h6",
  store: "M4 10v11h16V10 M3 10l2-7h14l2 7 M3 10h18 M9 21v-7h6v7",
  tracking: "M3 19V5 M3 19h18 M7 15l4-6 4 3 5-8 M16 4h4v4",
  inventory: "M3 7l9-4 9 4-9 4z M3 7v10l9 4 9-4V7 M12 11v10 M7 5l10 4",
  customers: "M9 12a4 4 0 1 0 0-8 4 4 0 0 0 0 8 M2 21v-2a6 6 0 0 1 12 0v2 M17 5a3 3 0 0 1 0 6 M18 15a5 5 0 0 1 4 5",
  operations: "M9 3h6v4H9z M5 5H3v16h18V5h-2 M7 12h10 M7 16h6",
  menu: "M4 6h16 M4 12h16 M4 18h16",
  chevron: "M14 6l-6 6 6 6",
  close: "M6 6l12 12 M18 6L6 18",
  arrow: "M5 12h14 M13 6l6 6-6 6",
};

export function AppIcon({ name, ...props }: SVGProps<SVGSVGElement> & { name: IconName }) {
  return <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" {...props}><path d={paths[name]} /></svg>;
}

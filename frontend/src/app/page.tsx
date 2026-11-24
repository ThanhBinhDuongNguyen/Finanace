import { FinanceDashboard } from "../features/dashboard/components/FinanceDashboard";
import { FinanceShell } from "../components/layout/FinanceShell";

export default function HomePage() {
  return <FinanceShell><FinanceDashboard /></FinanceShell>;
}

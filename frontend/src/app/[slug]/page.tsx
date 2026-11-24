import { notFound } from "next/navigation";
import { FinanceShell } from "../../components/layout/FinanceShell";
import { BalanceSheetReport } from "../../features/balance-sheet/components/BalanceSheetReport";
import { CashFlowReport } from "../../features/cash-flow/components/CashFlowReport";
import { ChannelReport } from "../../features/channels/components/ChannelReport";
import { StoreReport } from "../../features/stores/components/StoreReport";
import { StoreTrackingReport } from "../../features/store-tracking/components/StoreTrackingReport";
import { InventoryGroupReport } from "../../features/inventory-groups/components/InventoryGroupReport";
import { CustomerReport } from "../../features/customers/components/CustomerReport";
import { FinancialHealthReport } from "../../features/financial-health/components/FinancialHealthReport";
import { ProfitLossReport } from "../../features/profit-loss/components/ProfitLossReport";
import { FinanceOperations } from "../../features/finance-operations/components/FinanceOperations";
import { financeNavigation } from "../../routes/navigation";

type ReportPageProps = { params: Promise<{ slug: string }> };

export function generateStaticParams() {
  return financeNavigation.filter((item) => item.slug !== "overview").map((item) => ({ slug: item.slug }));
}

export default async function ReportPage({ params }: ReportPageProps) {
  const { slug } = await params;
  const report = financeNavigation.find((item) => item.slug === slug);
  if (!report || slug === "overview") notFound();

  let content = <div className="rounded-xl border border-blue-200 bg-blue-50 px-4 py-3 text-sm text-blue-950">Trang <strong>{report.label}</strong> sẽ được triển khai theo ảnh tiếp theo.</div>;
  if (slug === "lai-lo") content = <ProfitLossReport />;
  if (slug === "can-doi-ke-toan") content = <BalanceSheetReport />;
  if (slug === "luu-chuyen-tien") content = <CashFlowReport />;
  if (slug === "suc-khoe-tai-chinh") content = <FinancialHealthReport />;
  if (slug === "kenh-ban") content = <ChannelReport />;
  if (slug === "cua-hang") content = <StoreReport />;
  if (slug === "theo-doi-cua-hang") content = <StoreTrackingReport />;
  if (slug === "nhom-hang-ton-kho") content = <InventoryGroupReport />;
  if (slug === "khach-hang") content = <CustomerReport />;
  if (slug === "van-hanh-finance") content = <FinanceOperations />;

  return <FinanceShell>{content}</FinanceShell>;
}

import type { ChannelMix, Metric, MonthlyPerformance, StoreProfit } from "../types/dashboard";

export const periods = ["12/2026", "11/2026", "10/2026"];

export const metrics: Metric[] = [
  { label: "Doanh thu kỳ", value: "15.194" },
  { label: "Lãi gộp kỳ", value: "6.998", tone: "positive" },
  { label: "EBITDA kỳ", value: "2.709", tone: "positive" },
  { label: "LN sau thuế kỳ", value: "1.910", tone: "positive" },
  { label: "Doanh thu lũy kế", value: "128.306" },
  { label: "LN sau thuế lũy kế", value: "7.742", tone: "positive" },
  { label: "Biên lãi gộp", value: "46,1%" },
  { label: "Biên LN ròng", value: "12,6%" },
  { label: "Tiền cuối kỳ", value: "7.397" },
  { label: "Thanh toán hiện hành", value: "1,89x" },
  { label: "Số ngày tồn kho", value: "81", tone: "warning" },
  { label: "Tồn > 90 ngày", value: "17,1%", tone: "warning" },
  { label: "Footfall cửa hàng", value: "99.043" },
  { label: "Chuyển đổi cửa hàng", value: "18,6%", tone: "positive" },
  { label: "Bill TB (nghìn đ)", value: "643" },
  { label: "Tỷ trọng online", value: "18,5%" },
];

export const monthlyPerformance: MonthlyPerformance[] = [
  { month: "01/2026", revenue: 11_500, profit: 900 }, { month: "02/2026", revenue: 8_200, profit: 150 },
  { month: "03/2026", revenue: 9_100, profit: 340 }, { month: "04/2026", revenue: 9_400, profit: 400 },
  { month: "05/2026", revenue: 9_700, profit: 480 }, { month: "06/2026", revenue: 9_500, profit: 430 },
  { month: "07/2026", revenue: 9_600, profit: 470 }, { month: "08/2026", revenue: 10_300, profit: 610 },
  { month: "09/2026", revenue: 10_100, profit: 590 }, { month: "10/2026", revenue: 11_300, profit: 890 },
  { month: "11/2026", revenue: 12_200, profit: 1_180 }, { month: "12/2026", revenue: 15_194, profit: 1_910 },
];

export const channelMix: ChannelMix[] = [
  { name: "Cửa hàng", value: 78, color: "var(--chart-1)" },
  { name: "Website", value: 5, color: "var(--chart-4)" },
  { name: "Shopee", value: 10, color: "var(--chart-2)" },
  { name: "TikTok Shop", value: 7, color: "var(--chart-3)" },
];

export const storeProfit: StoreProfit[] = [
  { name: "CH01 Q1 – Nguyễn Trãi", value: 1_050 }, { name: "CH02 Q7 – Crescent Mall", value: 680 },
  { name: "CH03 Thủ Đức – Vincom", value: 720 }, { name: "CH04 Hà Nội – Phố Huế", value: 500 },
  { name: "CH05 Đà Nẵng – Lê Duẩn", value: 200 },
];

export const grossMargins = [
  { name: "Thời trang nữ", value: 48 }, { name: "Thời trang nam", value: 45 },
  { name: "Giày dép", value: 42 }, { name: "Túi & phụ kiện", value: 55 }, { name: "Mỹ phẩm", value: 37 },
];

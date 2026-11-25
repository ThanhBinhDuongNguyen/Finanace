import { ReportFrame } from "../../../components/ui/ReportFrame";

type RowKind = "group" | "total" | "standard" | "check";

type BalanceSheetRow = { label: string; kind: RowKind; values?: string[] };

const months = ["12/2025", "01/2026", "02/2026", "03/2026", "04/2026", "05/2026", "06/2026", "07/2026", "08/2026", "09/2026", "10/2026", "11/2026", "12/2026"];

const rows: BalanceSheetRow[] = [
  { label: "TÀI SẢN", kind: "group" },
  { label: "Tiền và tương đương tiền", kind: "standard", values: ["3.500", "4.957,2", "6.657,6", "6.869,2", "6.262,1", "6.900,6", "7.244,9", "5.960,8", "6.050,3", "6.768,7", "6.468,9", "7.175", "7.396,8"] },
  { label: "Phải thu khách hàng", kind: "standard", values: ["900", "794,3", "591,1", "710,6", "675,2", "768,5", "691,7", "855,7", "821,1", "866,1", "972,4", "1.005,4", "1.267,9"] },
  { label: "Hàng tồn kho", kind: "standard", values: ["15.916,2", "15.856,2", "11.726,3", "12.547", "13.396,3", "13.450,1", "13.465,7", "14.548,6", "15.661,6", "14.986,4", "16.909,3", "18.303,5", "22.075,6"] },
  { label: "Tài sản khác", kind: "standard", values: ["800", "800", "800", "800", "800", "800", "800", "800", "800", "800", "800", "800", "800"] },
  { label: "Tài sản ngắn hạn", kind: "total", values: ["21.116,2", "22.407,7", "19.774", "20.926,8", "21.133,6", "21.919,2", "22.202,3", "22.165,1", "23.333", "23.421,2", "25.150,6", "27.283,9", "31.540,3"] },
  { label: "Tài sản cố định ròng", kind: "standard", values: ["6.500", "6.265", "6.030", "5.795", "6.130", "5.895", "5.660", "5.425", "5.190", "5.525", "5.290", "5.055", "4.820"] },
  { label: "TỔNG TÀI SẢN", kind: "total", values: ["27.616,2", "28.672,7", "25.804", "26.721,8", "27.263,6", "27.814,2", "27.862,3", "27.540,1", "28.523", "28.946,2", "30.440,6", "32.338,9", "36.360,3"] },
  { label: "NGUỒN VỐN", kind: "group" },
  { label: "Phải trả nhà cung cấp", kind: "standard", values: ["8.749,4", "8.749,4", "6.253,1", "6.960,1", "7.260,8", "7.477,3", "7.268,1", "7.571,6", "8.048,3", "7.991,3", "8.657,7", "9.457,1", "11.474,3"] },
  { label: "Vay ngắn hạn", kind: "standard", values: ["5.000", "5.000", "5.000", "5.000", "5.000", "5.000", "5.000", "4.000", "4.000", "4.000", "4.000", "4.000", "4.000"] },
  { label: "Phải trả khác", kind: "standard", values: ["700", "931,3", "700", "762,2", "780,7", "803", "792,1", "813,3", "843,9", "844,8", "914,6", "982,7", "1.177,4"] },
  { label: "Nợ ngắn hạn", kind: "total", values: ["14.449,4", "14.680,7", "11.953,1", "12.722,3", "13.041,5", "13.280,3", "13.060,2", "12.384,9", "12.892,2", "12.836,1", "13.572,3", "14.439,8", "16.651,7"] },
  { label: "Vay dài hạn", kind: "standard", values: ["4.800", "4.700", "4.600", "4.500", "4.400", "4.300", "4.200", "4.100", "4.000", "3.900", "3.800", "3.700", "3.600"] },
  { label: "Tổng nợ phải trả", kind: "total", values: ["19.249,4", "19.380,7", "16.553,1", "17.222,3", "17.441,5", "17.580,3", "17.260,2", "16.484,9", "16.892,2", "16.736,1", "17.372,3", "18.139,8", "20.251,7"] },
  { label: "Vốn góp chủ sở hữu", kind: "standard", values: ["6.000", "6.000", "6.000", "6.000", "6.000", "6.000", "6.000", "6.000", "6.000", "6.000", "6.000", "6.000", "6.000"] },
  { label: "LN sau thuế chưa phân phối", kind: "standard", values: ["2.366,8", "3.292", "3.250,9", "3.499,5", "3.822,1", "4.233,9", "4.602,1", "5.055,2", "5.630,8", "6.210,1", "7.068,3", "8.199,1", "10.108,6"] },
  { label: "Vốn chủ sở hữu", kind: "total", values: ["8.366,8", "9.292", "9.250,9", "9.499,5", "9.822,1", "10.233,9", "10.602,1", "11.055,2", "11.630,8", "12.210,1", "13.068,3", "14.199,1", "16.108,6"] },
  { label: "TỔNG NGUỒN VỐN", kind: "total", values: ["27.616,2", "28.672,7", "25.804", "26.721,8", "27.263,6", "27.814,2", "27.862,3", "27.540,1", "28.523", "28.946,2", "30.440,6", "32.338,9", "36.360,3"] },
  { label: "Kiểm tra (phải = 0)", kind: "check", values: Array(13).fill("-") },
];

const rowClass: Record<RowKind, string> = {
  group: "finance-row-group bg-[#d9e5f1] font-bold text-[#17375e]",
  total: "finance-row-total font-bold text-slate-950",
  standard: "text-slate-700",
  check: "italic text-slate-500",
};

export function BalanceSheetReport() {
  return (
    <ReportFrame title="Bảng cân đối kế toán cuối tháng" description="Đơn vị tài chính: triệu đồng; tỷ lệ và số lượng theo tiêu đề cột." scrollHint>
      <div className="finance-table-scroll" tabIndex={0} role="region" aria-label="Bảng số liệu có thể cuộn ngang">
        <table className="finance-table min-w-[1280px]"><caption className="sr-only">Bảng cân đối kế toán cuối tháng</caption>
          <thead className="bg-[#203f70] text-white"><tr><th scope="col" className="sticky left-0 z-10 min-w-[var(--report-label-width)] bg-[#203f70] px-2 py-1.5 text-center font-semibold">Chỉ tiêu</th>{months.map((month) => <th scope="col" key={month} className="min-w-20 px-2 py-1.5 font-semibold">{month}</th>)}</tr></thead>
          <tbody>{rows.map((row) => row.kind === "group" ? <tr key={row.label} className={rowClass.group}><td className="sticky left-0 z-10 min-w-[var(--report-label-width)] bg-[#d9e5f1] px-2 py-1 text-left">{row.label}</td><td colSpan={13} className="bg-[#d9e5f1] px-2 py-1" /></tr> : <tr key={row.label} className={`border-b border-slate-200 ${rowClass[row.kind]}`}><td className="sticky left-0 bg-white px-2 py-1 text-left whitespace-nowrap">{row.label}</td>{row.values?.map((value, index) => <td key={`${row.label}-${months[index]}`} className="px-2 py-1 whitespace-nowrap">{value}</td>)}</tr>)}</tbody>
        </table>
      </div>
    </ReportFrame>
  );
}

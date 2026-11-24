type RowKind = "group" | "total" | "standard";
type CashFlowRow = { label: string; kind: RowKind; values?: string[] };

const months = ["01/2026", "02/2026", "03/2026", "04/2026", "05/2026", "06/2026", "07/2026", "08/2026", "09/2026", "10/2026", "11/2026", "12/2026", "Cả năm"];

const rows: CashFlowRow[] = [
  { label: "HOẠT ĐỘNG KINH DOANH", kind: "group" },
  { label: "Lợi nhuận sau thuế", kind: "standard", values: ["925,2", "(41,1)", "248,6", "322,6", "411,8", "368,2", "453,1", "575,6", "579,3", "858,2", "1.130,8", "1.909,5", "7.742"] },
  { label: "Cộng: khấu hao", kind: "standard", values: ["265", "265", "265", "265", "265", "265", "265", "265", "265", "265", "265", "265", "3.180"] },
  { label: "(Tăng)/giảm phải thu", kind: "standard", values: ["105,7", "204,2", "(119,5)", "35,4", "(93,3)", "(76,8)", "(164)", "(15,4)", "(45)", "(106,3)", "(33)", "(262,5)", "(367,9)"] },
  { label: "(Tăng)/giảm tồn kho", kind: "standard", values: ["60", "4.129,9", "(820,7)", "(849,3)", "(53,8)", "(15,6)", "(1.082,9)", "(1.113)", "675,2", "(1.922,9)", "(1.394,2)", "(3.772,1)", "(6.159,4)"] },
  { label: "(Tăng)/giảm tài sản khác", kind: "standard", values: Array(13).fill("-") },
  { label: "Tăng/(giảm) phải trả NCC", kind: "standard", values: ["-", "(2.496,3)", "707", "300,7", "216,5", "(209,2)", "303,5", "476,7", "(57)", "666,4", "799,4", "2.017,2", "2.724,9"] },
  { label: "Tăng/(giảm) phải trả khác", kind: "standard", values: ["231,3", "(231,3)", "62,2", "18,5", "22,3", "(10,9)", "21,2", "30,6", "0,9", "69,8", "68,1", "194,7", "477,4"] },
  { label: "Lưu chuyển tiền từ kinh doanh", kind: "total", values: ["1.587,2", "1.830,4", "341,6", "92,9", "768,5", "474,3", "(154,1)", "219,5", "1.418,4", "(169,8)", "836,1", "351,8", "7.597"] },
  { label: "HOẠT ĐỘNG ĐẦU TƯ", kind: "group" },
  { label: "Mua TSCĐ", kind: "total", values: ["(30)", "(30)", "(30)", "(600)", "(30)", "(30)", "(30)", "(30)", "(600)", "(30)", "(30)", "(30)", "(1.500)"] },
  { label: "HOẠT ĐỘNG TÀI CHÍNH", kind: "group" },
  { label: "Vay/(trả) ngắn hạn", kind: "standard", values: ["-", "-", "-", "-", "-", "-", "(1.000)", "-", "-", "-", "-", "-", "(1.000)"] },
  { label: "Vay/(trả) dài hạn", kind: "standard", values: Array(12).fill("(100)").concat("(1.200)") },
  { label: "Góp vốn/(chia lợi nhuận)", kind: "standard", values: Array(12).fill("-").concat("(0,2)") },
  { label: "Lưu chuyển tiền từ tài chính", kind: "total", values: ["(100)", "(100)", "(100)", "(100)", "(100)", "(100)", "(1.100)", "(100)", "(100)", "(100)", "(100)", "(100)", "(2.200,2)"] },
  { label: "LƯU CHUYỂN TIỀN THUẦN", kind: "total", values: ["1.457,2", "1.700,4", "211,6", "(607,1)", "638,5", "344,3", "(1.284,1)", "89,5", "718,4", "(299,8)", "706,1", "221,8", "3.896,8"] },
  { label: "Tiền đầu kỳ", kind: "standard", values: ["3.500", "4.957,2", "6.657,6", "6.869,2", "6.262,1", "6.900,6", "7.244,9", "5.960,8", "6.050,3", "6.768,7", "6.468,9", "7.175", "3.500"] },
  { label: "Tiền cuối kỳ", kind: "total", values: ["4.957,2", "6.657,6", "6.869,2", "6.262,1", "6.900,6", "7.244,9", "5.960,8", "6.050,3", "6.768,7", "6.468,9", "7.175", "7.396,8", "7.396,8"] },
];

const rowClass: Record<RowKind, string> = { group: "bg-[#d9e5f1] font-bold text-[#17375e]", total: "font-bold text-slate-950", standard: "text-slate-700" };

export function CashFlowReport() {
  return <section className="overflow-hidden rounded-xl border border-slate-200 bg-white p-4 shadow-sm sm:p-5"><h2 className="mb-3 text-base font-bold text-[#17375e]">Lưu chuyển tiền tệ (gián tiếp)</h2><div className="overflow-x-auto"><table className="min-w-[1280px] w-full border-collapse text-right text-sm"><thead className="bg-[#203f70] text-white"><tr><th className="sticky left-0 z-10 min-w-72 bg-[#203f70] px-2 py-1.5 text-center font-semibold">Chỉ tiêu</th>{months.map((month) => <th key={month} className="min-w-20 px-2 py-1.5 font-semibold">{month}</th>)}</tr></thead><tbody>{rows.map((row) => row.kind === "group" ? <tr key={row.label} className={rowClass.group}><td className="sticky left-0 z-10 min-w-72 bg-[#d9e5f1] px-2 py-1 text-left">{row.label}</td><td colSpan={13} className="bg-[#d9e5f1] px-2 py-1" /></tr> : <tr key={row.label} className={`border-b border-slate-200 ${rowClass[row.kind]}`}><td className="sticky left-0 bg-white px-2 py-1 text-left whitespace-nowrap">{row.label}</td>{row.values?.map((value, index) => <td key={`${row.label}-${months[index]}`} className={`px-2 py-1 whitespace-nowrap ${value.startsWith("(") ? "text-red-600" : ""}`}>{value}</td>)}</tr>)}</tbody></table></div></section>;
}

import { ReportFrame } from "../../../components/ui/ReportFrame";

type RowKind = "group" | "total" | "margin" | "standard";

type ProfitLossRow = {
  label: string;
  kind: RowKind;
  values?: string[];
};

const months = ["01/2026", "02/2026", "03/2026", "04/2026", "05/2026", "06/2026", "07/2026", "08/2026", "09/2026", "10/2026", "11/2026", "12/2026", "Cả năm"];

const rows: ProfitLossRow[] = [
  { label: "DOANH THU & LÃI GỘP", kind: "group" },
  { label: "Doanh thu thuần", kind: "total", values: ["11.555,9", "8.256,8", "9.228,9", "9.525,8", "9.869,1", "9.620,6", "10.017", "10.563,2", "10.536,8", "11.461,5", "12.476,3", "15.194,2", "128.306,1"] },
  { label: "Giá vốn hàng bán", kind: "standard", values: ["6.249,6", "4.466,5", "4.971,5", "5.186,3", "5.340,9", "5.191,5", "5.408,3", "5.748,8", "5.708,1", "6.184,1", "6.755,1", "8.195,9", "69.416,6"] },
  { label: "Lãi gộp", kind: "total", values: ["5.306,3", "3.790,3", "4.257,4", "4.339,5", "4.528,2", "4.429,1", "4.608,7", "4.814,4", "4.828,7", "5.277,4", "5.721,2", "6.998,3", "58.889,5"] },
  { label: "Biên lãi gộp", kind: "margin", values: ["45,9%", "45,9%", "46,1%", "45,6%", "45,9%", "46,0%", "46,0%", "45,6%", "45,8%", "46,0%", "45,9%", "46,1%", "45,9%"] },
  { label: "CHI PHÍ HOẠT ĐỘNG", kind: "group" },
  { label: "Chi phí bán hàng (cửa hàng + online)", kind: "standard", values: ["2.878,1", "2.609,9", "2.711,4", "2.697,3", "2.770,2", "2.730", "2.798,3", "2.850,7", "2.861,5", "2.948,5", "3.037,2", "3.300,9", "34.194"] },
  { label: "Lợi nhuận đóng góp của các kênh", kind: "total", values: ["2.428,2", "1.180,4", "1.546", "1.642,2", "1.758", "1.699,1", "1.810,4", "1.963,7", "1.967,2", "2.328,9", "2.684", "3.697,4", "24.705,5"] },
  { label: "Chi phí quản lý (văn phòng, kho)", kind: "standard", values: ["933,3", "883,9", "898,4", "902,9", "908", "904,3", "910,3", "918,4", "918,1", "931,9", "947,1", "987,9", "11.044,5"] },
  { label: "EBITDA", kind: "total", values: ["1.494,9", "296,5", "647,6", "739,3", "850", "794,8", "900,1", "1.045,3", "1.049,1", "1.397", "1.736,9", "2.709,5", "13.661"] },
  { label: "Biên EBITDA", kind: "margin", values: ["12,9%", "3,6%", "7,0%", "7,8%", "8,6%", "8,3%", "9,0%", "9,9%", "10,0%", "12,2%", "13,9%", "17,8%", "10,6%"] },
  { label: "Khấu hao", kind: "standard", values: ["265", "265", "265", "265", "265", "265", "265", "265", "265", "265", "265", "265", "3.180"] },
  { label: "EBIT", kind: "total", values: ["1.229,9", "31,5", "382,6", "474,3", "585", "529,8", "635,1", "780,3", "784,1", "1.132", "1.471,9", "2.444,5", "10.481"] },
  { label: "Chi phí lãi vay", kind: "standard", values: ["73,4", "72,6", "71,8", "71", "70,2", "69,5", "68,7", "60,8", "60", "59,2", "58,4", "57,6", "793,2"] },
  { label: "Lợi nhuận trước thuế", kind: "total", values: ["1.156,5", "(41,1)", "310,8", "403,3", "514,8", "460,3", "566,4", "719,5", "724,1", "1.072,8", "1.413,5", "2.386,9", "9.687,8"] },
  { label: "Thuế TNDN (20%)", kind: "standard", values: ["231,3", "-", "62,2", "80,7", "103", "92,1", "113,3", "143,9", "144,8", "214,6", "282,7", "477,4", "1.945,8"] },
  { label: "LỢI NHUẬN SAU THUẾ", kind: "total", values: ["925,2", "(41,1)", "248,6", "322,6", "411,8", "368,2", "453,1", "575,6", "579,3", "858,2", "1.130,8", "1.909,5", "7.742"] },
  { label: "Biên lợi nhuận ròng", kind: "margin", values: ["8,0%", "-0,5%", "2,7%", "3,4%", "4,2%", "3,8%", "4,5%", "5,4%", "5,5%", "7,5%", "9,1%", "12,6%", "6,0%"] },
];

const rowClass: Record<RowKind, string> = {
  group: "finance-row-group bg-[var(--brand-soft)] font-bold text-[var(--brand)]",
  total: "finance-row-total font-bold text-slate-950",
  margin: "italic text-slate-600",
  standard: "text-slate-700",
};

export function ProfitLossReport() {
  return (
    <ReportFrame title="Báo cáo kết quả kinh doanh theo tháng" description="Đơn vị tài chính: triệu đồng; tỷ lệ và số lượng theo tiêu đề cột." scrollHint>
      <div className="finance-table-scroll" tabIndex={0} role="region" aria-label="Bảng số liệu có thể cuộn ngang">
        <table className="finance-table min-w-[1280px]"><caption className="sr-only">Báo cáo kết quả kinh doanh theo tháng</caption>
          <thead className="bg-[var(--brand)] text-white">
            <tr>
              <th scope="col" className="sticky left-0 z-10 min-w-[var(--report-label-width)] bg-[var(--brand)] px-2 py-1.5 text-center font-semibold">Chỉ tiêu</th>
              {months.map((month) => <th scope="col" key={month} className="min-w-20 px-2 py-1.5 font-semibold">{month}</th>)}
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => row.kind === "group" ? (
              <tr key={row.label} className={rowClass.group}>
                <td className="sticky left-0 z-10 min-w-[var(--report-label-width)] bg-[var(--brand-soft)] px-2 py-1 text-left">{row.label}</td>
                <td colSpan={13} className="bg-[var(--brand-soft)] px-2 py-1" />
              </tr>
            ) : (
              <tr key={row.label} className={`border-b border-slate-200 ${rowClass[row.kind]}`}>
                <td className="sticky left-0 bg-white px-2 py-1 text-left whitespace-nowrap">{row.label}</td>
                {row.values?.map((value, index) => <td key={`${row.label}-${months[index]}`} className={`px-2 py-1 whitespace-nowrap ${value.startsWith("(") || value.startsWith("-") ? "text-red-600" : ""}`}>{value}</td>)}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <p className="mt-4 text-sm text-slate-600">Thuế TNDN tính đơn giản 20% trên lãi từng tháng, để tham khảo quản trị.</p>
    </ReportFrame>
  );
}

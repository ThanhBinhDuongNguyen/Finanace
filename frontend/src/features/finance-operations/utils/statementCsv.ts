import type { ImportBatchInput } from "../../../services/finance/types";

// RFC-style quoted fields, doubled quotes and CRLF; imported values are never evaluated.
export function parseStatementCsv(csv: string): ImportBatchInput["lines"] {
  const rows: string[][] = [];
  let row: string[] = [], field = "", quoted = false;
  const text = csv.replace(/^\uFEFF/, "");
  for (let i = 0; i < text.length; i++) {
    const c = text[i];
    if (c === '"') {
      if (quoted && text[i + 1] === '"') { field += '"'; i++; }
      else quoted = !quoted;
    } else if (c === "," && !quoted) { row.push(field); field = ""; }
    else if (c === "\n" && !quoted) { row.push(field.replace(/\r$/, "")); rows.push(row); row = []; field = ""; }
    else field += c;
  }
  if (quoted) throw new Error("CSV có dấu ngoặc kép chưa đóng.");
  if (field || row.length) { row.push(field.replace(/\r$/, "")); rows.push(row); }
  if (rows[0]?.[0]?.trim().toLowerCase() === "externalid") rows.shift();
  const result = rows.filter((r) => r.some((v) => v.trim())).map((r, index) => {
    if (r.length !== 4) throw new Error(`Dòng ${index + 1}: cần 4 cột externalId,amount,occurredOn,businessType.`);
    const [externalId, amount, occurredOn, businessType] = r.map((v) => v.trim());
    if (!amount || !Number.isFinite(Number(amount))) throw new Error(`Dòng ${index + 1}: số tiền không hợp lệ.`);
    const date = new Date(occurredOn);
    if (!/^\d{4}-\d{2}-\d{2}$/.test(occurredOn) || !Number.isFinite(date.valueOf()) || date.toISOString().slice(0, 10) !== occurredOn) throw new Error(`Dòng ${index + 1}: ngày phải theo định dạng yyyy-MM-dd và tồn tại trong lịch.`);
    return { externalId, amount: Number(amount), occurredOn, businessType, rawData: r.join(",") };
  });
  if (!result.length) throw new Error("CSV chưa có dòng dữ liệu.");
  if (result.length > 10000) throw new Error("Mỗi lần nhập tối đa 10.000 dòng.");
  return result;
}

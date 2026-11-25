import assert from "node:assert/strict";
import { test } from "node:test";
import { parseStatementCsv } from "../src/features/finance-operations/utils/statementCsv.ts";

test("CSV supports BOM, header, CRLF, quoted commas and signed money", () => {
  const lines = parseStatementCsv('\uFEFFexternalId,amount,occurredOn,businessType\r\n"PAY,001",1000000,2026-12-12,Revenue\r\nFEE,-10000,2026-12-12,"Platform ""fee"""');
  assert.equal(lines.length, 2);
  assert.equal(lines[0].externalId, "PAY,001");
  assert.equal(lines[1].amount, -10000);
  assert.equal(lines[1].businessType, 'Platform "fee"');
});

test("CSV retains missing source ID for server quarantine rather than inventing it", () => {
  assert.equal(parseStatementCsv(",10,2026-12-12,Fee")[0].externalId, "");
});

test("CSV rejects invalid money, impossible dates, malformed quotes and wrong shape", () => {
  for (const csv of ["", "A,NaN,2026-12-12,Fee", "A,,2026-12-12,Fee", "A,10,2026-02-30,Fee", "A,10,2026-12-12", '"A,10,2026-12-12,Fee']) {
    assert.throws(() => parseStatementCsv(csv));
  }
});

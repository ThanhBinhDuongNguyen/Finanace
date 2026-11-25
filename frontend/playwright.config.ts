import { defineConfig, devices } from "@playwright/test";
import { mkdtempSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";

const database = join(mkdtempSync(join(tmpdir(), "finance-browser-")), "finance.db");

export default defineConfig({
  testDir: "./tests/e2e",
  fullyParallel: false,
  workers: 1,
  retries: 0,
  timeout: 90_000,
  expect: { timeout: 15_000 },
  use: { baseURL: "http://127.0.0.1:3100", trace: "retain-on-failure", screenshot: "only-on-failure" },
  projects: [{ name: "chromium", use: { ...devices["Desktop Chrome"] } }],
  webServer: [
    {
      command: `dotnet run --no-build --no-launch-profile --project ../backend/Finance.Api --urls http://127.0.0.1:5082 --ConnectionStrings:FinanceDb="Data Source=${database}"`,
      url: "http://127.0.0.1:5082/swagger/index.html",
      reuseExistingServer: false,
      timeout: 90_000,
      env: { ASPNETCORE_ENVIRONMENT: "Development" },
    },
    {
      command: "npm run dev -- --hostname 127.0.0.1 --port 3100",
      url: "http://127.0.0.1:3100/van-hanh-finance",
      reuseExistingServer: false,
      timeout: 120_000,
      env: { FINANCE_API_ORIGIN: "http://127.0.0.1:5082" },
    },
  ],
});

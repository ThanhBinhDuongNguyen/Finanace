import { expect, test } from "@playwright/test";

const reports = ["/", "/lai-lo", "/can-doi-ke-toan", "/luu-chuyen-tien", "/suc-khoe-tai-chinh", "/kenh-ban", "/cua-hang", "/theo-doi-cua-hang", "/nhom-hang-ton-kho", "/khach-hang"];

test("all report routes render and contain horizontal overflow at desktop and mobile", async ({ page }, info) => {
  const pageErrors: string[] = [];
  page.on("pageerror", error => pageErrors.push(error.message));
  for (const width of [1440, 390]) {
    await page.setViewportSize({ width, height: 900 });
    for (const path of reports) {
      await page.goto(path);
      await expect(page.locator("h1")).toBeVisible();
      await expect(page.getByRole("main")).toBeVisible();
      expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth + 1), `${path} at ${width}px overflows page`).toBeTruthy();
      const charts = page.locator(".finance-chart");
      if (await charts.count()) {
        await expect(charts.first().locator("svg.recharts-surface").first()).toBeVisible();
        expect(await charts.first().evaluate(node => node.getBoundingClientRect().height)).toBeGreaterThan(200);
      }
      if (path === "/" || path === "/lai-lo") await page.screenshot({ path: info.outputPath(`${path === "/" ? "overview" : "profit-loss"}-${width}.png`), fullPage: true });
    }
  }
  expect(pageErrors).toEqual([]);
});

test("sidebar persists collapsed preference; mobile drawer traps focus and closes with Escape", async ({ page }) => {
  await page.setViewportSize({ width: 1440, height: 900 });
  await page.goto("/");
  await page.getByRole("button", { name: "Thu gọn sidebar", exact: true }).click();
  await expect(page.getByRole("button", { name: "Mở sidebar", exact: true })).toHaveAttribute("aria-expanded", "false");
  await page.reload();
  await expect(page.getByRole("button", { name: "Mở sidebar", exact: true })).toBeVisible();
  await page.setViewportSize({ width: 390, height: 844 });
  const open = page.getByRole("button", { name: "Mở menu điều hướng", exact: true });
  await open.click();
  const dialog = page.getByRole("dialog", { name: "Menu điều hướng", exact: true });
  await expect(dialog).toBeVisible();
  await page.keyboard.press("Shift+Tab");
  expect(await page.evaluate(() => document.querySelector("dialog")?.contains(document.activeElement))).toBeTruthy();
  await page.keyboard.press("Escape");
  await expect(dialog).not.toBeVisible();
  await expect(open).toBeFocused();
  await open.click();
  await dialog.getByRole("link", { name: "Lãi lỗ", exact: true }).click();
  await expect(page).toHaveURL(/lai-lo/);
  await expect(dialog).not.toBeVisible();
});

test("table label stays visible after horizontal scroll", async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto("/lai-lo");
  const region = page.locator(".finance-table-scroll");
  await region.evaluate(node => { node.scrollLeft = 700; });
  const label = page.getByRole("cell", { name: "DOANH THU & LÃI GỘP", exact: true });
  const bounds = await label.boundingBox();
  expect(bounds?.x).toBeGreaterThanOrEqual(0);
  expect((bounds?.x ?? 0) + (bounds?.width ?? 0)).toBeLessThan(390);
  await expect(label).toBeVisible();
});

test("operations keyboard tabs preserve a draft when changing workspace", async ({ page }, info) => {
  await page.setViewportSize({ width: 1024, height: 900 });
  await page.goto("/van-hanh-finance");
  await page.getByLabel("Tên đăng nhập", { exact: true }).fill("maker");
  await page.getByLabel("Mật khẩu", { exact: true }).fill("FinanceDemo!2026");
  await page.getByRole("button", { name: "Đăng nhập", exact: true }).click();
  await expect(page.getByRole("tab", { name: "Nhập dữ liệu", exact: true })).toBeVisible();
  await page.getByLabel("Nội dung CSV", { exact: true }).fill("draft,100,2026-12-01,Revenue");
  await page.getByRole("tab", { name: "Nhập dữ liệu", exact: true }).focus();
  await page.keyboard.press("ArrowRight");
  await expect(page.getByRole("tab", { name: "Đối soát", exact: true })).toHaveAttribute("aria-selected", "true");
  await expect(page.getByRole("tab", { name: "Đối soát", exact: true })).toBeFocused();
  await page.keyboard.press("Home");
  await expect(page.getByRole("tab", { name: "Nhập dữ liệu", exact: true })).toHaveAttribute("aria-selected", "true");
  await expect(page.getByLabel("Nội dung CSV", { exact: true })).toHaveValue("draft,100,2026-12-01,Revenue");
  await page.screenshot({ path: info.outputPath("operations-tablet.png"), fullPage: true });
  await page.setViewportSize({ width: 390, height: 844 });
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth + 1)).toBeTruthy();
  await page.screenshot({ path: info.outputPath("operations-mobile.png"), fullPage: true });
});

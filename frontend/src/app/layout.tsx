import type { Metadata } from "next";
import { StoreProvider } from "../stores/StoreProvider";
import { Suspense } from "react";
import "./globals.css";

export const metadata: Metadata = {
  icons: { icon: "/favicon.svg" },
  title: "Nexus Finance",
  description: "Báo cáo quản trị chuỗi bán lẻ",
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="vi">
      <body>
        <StoreProvider><Suspense fallback={<div className="mx-auto max-w-screen-xl p-8" role="status" aria-label="Đang tải trang"><div className="finance-skeleton mb-6 h-10 w-64" /><div className="finance-skeleton h-80 w-full" /><span className="sr-only">Đang tải trang…</span></div>}>{children}</Suspense></StoreProvider>
      </body>
    </html>
  );
}

import type { Metadata } from "next";
import { StoreProvider } from "../stores/StoreProvider";
import "./globals.css";

export const metadata: Metadata = {
  title: "Nexus Finance",
  description: "Báo cáo quản trị chuỗi bán lẻ",
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="vi">
      <body>
        <StoreProvider>{children}</StoreProvider>
      </body>
    </html>
  );
}

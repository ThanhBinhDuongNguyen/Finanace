import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  devIndicators: false,
  distDir: process.env.NODE_ENV === "development" ? ".next-dev" : ".next",
  async rewrites() {
    return [{ source: "/api/:path*", destination: `${process.env.FINANCE_API_ORIGIN ?? "http://127.0.0.1:5080"}/api/:path*` }];
  },
  eslint: {
    // Lint được chạy riêng bằng `npm run lint`; tránh Next 15 nạp lại ESLint sai môi trường.
    ignoreDuringBuilds: true,
  },
};

export default nextConfig;

import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  eslint: {
    // Lint được chạy riêng bằng `npm run lint`; tránh Next 15 nạp lại ESLint sai môi trường.
    ignoreDuringBuilds: true,
  },
};

export default nextConfig;

import axios from "axios";

export const http = axios.create({ baseURL: "/api", timeout: 10_000, withCredentials: true, headers: { "X-Finance-Client": "web" } });

export function apiError(error: unknown): string {
  if (!axios.isAxiosError(error)) return error instanceof Error ? error.message : "Có lỗi khi xử lý yêu cầu.";
  if (error.response?.status === 401) return "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
  if (error.response?.status === 403) return "Tài khoản không có quyền thực hiện thao tác hoặc truy cập shop này.";
  const data = error.response?.data;
  if (data?.errors) return Object.values(data.errors).flat().join(" ");
  return data?.message ?? data?.detail ?? data?.title ?? "Không kết nối được API. Kiểm tra backend và cấu hình địa chỉ API.";
}

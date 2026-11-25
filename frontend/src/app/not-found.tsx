import Link from "next/link";

export default function NotFound() {
  return <main className="mx-auto flex min-h-dvh max-w-xl flex-col items-start justify-center px-6 py-12"><span className="finance-status">404</span><h1 className="mt-5 text-3xl font-semibold tracking-tight text-slate-900">Không tìm thấy trang</h1><p className="mt-3 text-base text-slate-600">Đường dẫn có thể đã thay đổi. Bạn có thể quay về tổng quan hoặc mở khu vực vận hành để tiếp tục.</p><div className="mt-6 flex flex-wrap gap-3"><Link className="finance-button" href="/">Về tổng quan</Link><Link className="finance-button-secondary" href="/van-hanh-finance">Mở vận hành Finance</Link></div></main>;
}

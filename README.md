# Nexus Finance

Prototype Finance & Control cho báo cáo quản trị chuỗi bán lẻ, đối soát đơn,
payout, ngân hàng và các ngoại lệ.

## Chạy frontend

```powershell
cd frontend
npm install
npm run dev
```

Mở `http://localhost:3000`.

## Chạy backend

```powershell
dotnet run --project backend/Finance.Api --urls http://127.0.0.1:5080
```

Swagger chạy tại `http://127.0.0.1:5080/swagger`.
SQLite được tạo tự động tại `backend/Finance.Api/finance.db`, nâng cấp schema bằng EF Core Migrations và không được commit.

Vòng đầu đã có `POST`/`GET /api/finance/import-batches`: lưu batch import, đưa dòng thiếu mã/loại nghiệp vụ hoặc số tiền 0 vào quarantine, và trả batch cũ khi payload trùng checksum.

`POST`/`GET /api/finance/reconciliation/payouts`, `bank-transactions` và `cases` phục vụ đối soát payout-ngân hàng. Case vượt tolerance là `Disputed`; case trong tolerance cần người duyệt khác người đề xuất để thành `FullyMatched`.

`/api/finance/exceptions` quản lý ngoại lệ theo chuỗi `Open → PendingApproval → Closed`; đóng và mở lại đều cần evidence. `/api/finance/ledger` post journal cân Nợ/Có, khóa kỳ và tạo reversal. `/api/finance/classification` quản lý mapping phiên bản và đưa dòng chưa mapping vào exception.

Khi nối UI, Axios được tập trung tại `frontend/src/services`; component không gọi HTTP trực tiếp.
Thiết lập `frontend/.env.local` từ `.env.example`: `FINANCE_API_ORIGIN=http://127.0.0.1:5080`.
Axios gọi cùng origin `/api`, Next proxy đến backend để session cookie không bị khác host.
Dev dùng `.next-dev`, production dùng `.next`; có thể build khi dev chạy mà không ghi đè chunk.

## Đăng nhập và quyền

Trong môi trường Development và khi chưa cấu hình `FinanceUsers`, có các tài khoản `maker`, `checker`, `controller`, `admin`, `connector`, mật khẩu demo `FinanceDemo!2026`.
Đây là tài khoản local. Ngoài Development không có fallback demo; cấu hình danh sách `FinanceUsers` qua secret/environment với `Username`, `PasswordHash` (ASP.NET PasswordHasher), `Roles`, `Shops`.
API lấy danh tính từ session cookie. Các trường actor/createdBy/postedBy truyền từ UI không quyết định quyền.
Mọi request ghi cần header `X-Finance-Client: web`; Axios đã cấu hình sẵn. Chỉ các shop trong session được truy vấn và export.

| Role | Thao tác chính |
| --- | --- |
| Maker | Import, phân loại, đề xuất đối soát/ngoại lệ, post journal |
| Checker | Xác nhận đối soát, duyệt ngoại lệ/refund/chứng từ, duyệt chuyển nguồn |
| Controller | Khóa kỳ, reversal, duyệt mở kỳ, chính sách giá vốn |
| Admin | Mapping, tolerance, đề xuất chuyển nguồn/mở kỳ, cấu hình dữ liệu |
| Integration (`connector`) | Import nguồn Connected sau khi chuyển nguồn đã duyệt |

Hạn mức duyệt đặt qua `FinanceApprovalLimits` gồm `{Role, Operation, MaxAmount}`. Các operation đang dùng: `Reconciliation`, `ExceptionClosure`, `Refund`, `PayoutAccountChange` hoặc `*`.
Development mặc định Checker 10.000.000, Controller 1.000.000.000. Ngoài Development không có hạn mức fallback; phải cấu hình trước khi phê duyệt.
UI kiểm duyệt theo phiên, server kiểm role, số tiền và người đề xuất độc lập. Thay tài khoản nhận tiền giữ lịch sử, proposal chưa duyệt không tác động tài khoản active.

Không gộp role Maker và Checker cho cùng người nếu cần giữ kiểm soát bất kiêm nhiệm. Server vẫn chặn cùng danh tính tự duyệt.

## Chạy kiểm thử

Cần SDK .NET 10 (đọc solution `.slnx`), runtime .NET 8 và Node 22.

```powershell
dotnet test Finance.slnx --logger trx
cd frontend
npm test
npm run lint
npm run build
npx playwright install chromium
npm run test:e2e
```

Backend tests dùng SQLite tách biệt; browser tests tự chạy API ở 5082 và frontend ở 3100 với database tạm riêng. Không bật server thủ công trên hai cổng test này.
Kết quả backend nằm trong `tests/Finance.Tests/TestResults`, browser trace/screenshot khi lỗi nằm trong `frontend/test-results`.
CI có các job backend, frontend và Chromium; cần push để CI chạy, chưa có kết quả CI từ xa.

## Phạm vi và nghiệm thu

Xem `docs/IMPLEMENTATION_STATUS.md` để biết phần đã có kiểm thử và các điều kiện cần xác minh thêm. Dữ liệu demo không phải số liệu kế toán đã nghiệm thu.

## Quy chuẩn giao diện

- Màu, độ tương phản, khoảng cách, form, nút và bảng dùng token tại `frontend/src/styles/theme.css` và `frontend/src/app/globals.css`.
- Khung sidebar thu gọn được lưu trên trình duyệt; mobile dùng drawer hỗ trợ Escape và focus bàn phím.
- `/van-hanh-finance` vẫn là đường dẫn chính. Mỗi workspace có URL `?section=imports|reconciliation|exceptions|ledger|classification|report|extensions|controls`.
- Tab hỗ trợ ArrowLeft/ArrowRight/Home/End, giữ bản nháp khi đổi workspace trong phiên hiện tại. Tải lại hoặc rời trang không lưu bản nháp lên server.
- Báo cáo dùng `ReportFrame`, `MetricCard`, bảng có header/cột chỉ tiêu cố định; chi tiết nghiệp vụ dùng `DataDetail` và status badge thay JSON thô.
- Biểu đồ không nội suy số liệu bằng animation. Báo cáo minh họa và dữ liệu API được gắn nguồn riêng.
- `frontend/tests/e2e/ui.spec.ts` kiểm tra các route báo cáo ở 1440px/390px, menu di động, cuộn ngang bảng và giữ bản nháp; ảnh QA được ghi trong `frontend/test-results`.

## Kiểm tra end-to-end thủ công

1. Chạy backend rồi frontend theo hai lệnh ở trên.
2. Mở `/van-hanh-finance`, import statement mẫu.
3. Thêm mapping `Revenue`, chọn batch và chạy classification: dòng chưa mapping sẽ tạo exception.
4. Thêm payout/bank, tạo case; chênh lệch 10.000 với tolerance 0 phải là `Disputed`.
5. Post journal cân Nợ/Có; kiểm tra khóa kỳ chặn post mới qua Swagger.

Dashboard trang chủ lấy KPI vận hành từ `GET /api/finance/operations/summary`; biểu đồ báo cáo vẫn là dữ liệu minh hoạ.

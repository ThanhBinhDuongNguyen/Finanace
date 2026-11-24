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
dotnet run --project backend/Finance.Api
```

Swagger chạy tại `http://localhost:5000/swagger` (hoặc cổng được .NET in ra).
SQLite được tạo tự động tại `backend/Finance.Api/finance.db`, nâng cấp schema bằng EF Core Migrations và không được commit.

Vòng đầu đã có `POST`/`GET /api/finance/import-batches`: lưu batch import, đưa dòng thiếu mã/loại nghiệp vụ hoặc số tiền 0 vào quarantine, và trả batch cũ khi payload trùng checksum.

`POST`/`GET /api/finance/reconciliation/payouts`, `bank-transactions` và `cases` phục vụ đối soát payout-ngân hàng. Case vượt tolerance là `Disputed`; case trong tolerance cần người duyệt khác người đề xuất để thành `FullyMatched`.

`/api/finance/exceptions` quản lý ngoại lệ theo chuỗi `Open → PendingApproval → Closed`; đóng và mở lại đều cần evidence. `/api/finance/ledger` post journal cân Nợ/Có, khóa kỳ và tạo reversal. `/api/finance/classification` quản lý mapping phiên bản và đưa dòng chưa mapping vào exception.

Khi nối UI, Axios được tập trung tại `frontend/src/services`; component không gọi HTTP trực tiếp.
Thiết lập `frontend/.env.local` từ `.env.example` để frontend gọi API local.

## Kiểm tra end-to-end thủ công

1. Chạy backend rồi frontend theo hai lệnh ở trên.
2. Mở `/van-hanh-finance`, import statement mẫu.
3. Thêm mapping `Revenue`, chọn batch và chạy classification: dòng chưa mapping sẽ tạo exception.
4. Thêm payout/bank, tạo case; chênh lệch 10.000 với tolerance 0 phải là `Disputed`.
5. Post journal cân Nợ/Có; kiểm tra khóa kỳ chặn post mới qua Swagger.

Dashboard trang chủ lấy KPI vận hành từ `GET /api/finance/operations/summary`; biểu đồ báo cáo vẫn là dữ liệu minh hoạ.

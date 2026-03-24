# WebBanHang

Ứng dụng web thương mại điện tử (giỏ hàng) fullstack.

## 📁 Cấu trúc dự án
- `frontend/`: React + Vite
- `WebBanHang/`: Backend ASP.NET Core

## 🚀 Chạy ứng dụng
### Backend
1. Mở `WebBanHang` trong Visual Studio.
2. Chạy migrate database và seed (nếu cần).
3. Chạy ứng dụng.

### Frontend
```bash
cd frontend
npm install
npm run dev
```

## 🛠 Công nghệ
- Frontend: React, Vite, JavaScript
- Backend: ASP.NET Core, Entity Framework Core, Identity
- Database: SQLite/MSSQL (tùy cài đặt)

## 🔒 Cấu hình
- `frontend/.env` (nếu dùng)
- Backend: `appsettings.json` / `appsettings.Development.json`

## 💡 Lưu ý
- Đã có `.gitignore` hỗ trợ cả frontend + backend.
- Tùy chỉnh connection string và identity settings trước khi deploy.

## 🧩 Mở rộng
- Thêm phân trang, search, filter sản phẩm
- Tích hợp thanh toán (Stripe/PayPal)
- Quản trị sản phẩm + đơn hàng

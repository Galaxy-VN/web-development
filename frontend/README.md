# Product Frontend (React + Vite + Axios)

Ung dung frontend don gian de hien thi danh sach san pham tu API ASP.NET Core.

## Run

1. Chay backend:

```bash
dotnet run --project ../WebBanHang/WebBanHang.csproj
```

2. Chay frontend:

```bash
npm install
npm run dev
```

3. Mo trinh duyet tai:

```text
http://localhost:5173
```

## API config

- Mac dinh frontend goi `GET /api/products`.
- Vite dev server da proxy `/api` ve `http://localhost:5272` trong `vite.config.js`.
- Neu muon goi truc tiep mot base URL khac, tao file `.env`:

```env
VITE_API_BASE_URL=http://localhost:5272
```

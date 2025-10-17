# FUNewsManagement — Hướng dẫn nhanh

Tài liệu ngắn này mô tả cách cấu hình và chạy project, tài khoản dùng để test (Admin/Staff/Lecturer), và một số hướng dẫn kiểm thử nhanh.

## Yêu cầu
- .NET SDK 8.0 (hoặc tương thích với project net8.0)
- SQL Server (local hoặc remote)
- PowerShell (Windows)

## Cấu hình kết nối
1. Mở file `FUNewsManagement/appsettings.json`.
2. Chỉnh chuỗi kết nối `ConnectionStrings:DefaultConnection` tới SQL Server của bạn, ví dụ:

```json
"ConnectionStrings": {
  "DefaultConnection": "server=localhost;uid=sa;pwd=YourStrong!Pass;database=FUNewsManagement;TrustServerCertificate=true"
}
```

3. Nếu cần thay đổi thông tin admin mặc định, cập nhật `AdminAccount` trong `appsettings.json`.

## Tạo database / chạy migrations
- Project hiện tại dùng DbContext sinh bởi EF Core. Nếu bạn muốn tạo database từ code, dùng các lệnh EF Core (nếu có migrations trong project) hoặc chạy script SQL tương ứng.
- Ví dụ nhanh (nếu bạn đã cài EF tools và có migrations):

```powershell
cd D:\FPT\.Net\FUNewsManagement\DataAccessLayer
dotnet ef database update --project ..\DataAccessLayer\DataAccessLayer.csproj --startup-project ..\FUNewsManagement\FUNewsManagement.csproj
```

> Lưu ý: Nếu không có migrations sẵn, bạn có thể tạo migration bằng `dotnet ef migrations add Initial` trước khi `database update`.

## Chạy ứng dụng
Từ thư mục gốc workspace (nơi chứa `FUNewsManagement.sln`), chạy:

```powershell
cd D:\FPT\.Net\FUNewsManagement
dotnet build FUNewsManagement.sln
dotnet run --project FUNewsManagement\FUNewsManagement.csproj
```

Sau khi chạy, mở trình duyệt tới `https://localhost:5001` hoặc URL được hiển thị trong console.

## Tài khoản test (roles)
Project có cấu hình một Admin mặc định trong `appsettings.json`:

- Admin
  - Email: `admin@FUNewsManagementSystem.org`
  - Password: `@@abc123@@`

Lưu ý: Các tài khoản Staff / Lecturer không được liệt kê trong cấu hình. Để tạo các tài khoản này, đăng ký qua giao diện (nếu app hỗ trợ) hoặc thêm thủ công vào bảng `SystemAccount` trong database:

- Staff (ví dụ):
  - AccountEmail: staff@example.org
  - AccountPassword: <mật khẩu hash hoặc plaintext tùy cấu hình app>
  - AccountName: Staff User
  - Role: Staff (nếu có cột role)

- Lecturer (ví dụ):
  - AccountEmail: lecturer@example.org
  - AccountPassword: <mật khẩu>
  - AccountName: Lecturer User
  - Role: Lecturer

Nếu bạn muốn, mình có thể thêm script SQL mẫu để chèn 3 tài khoản (Admin/Staff/Lecturer) vào DB.

## Kiểm thử xóa bài viết / thẻ
- Đăng nhập với tài khoản Admin.
- Tạo 1 bài viết (NewsArticle) và gắn 1-2 tag.
- Xóa bài viết: kiểm tra bảng `NewsTag` không còn liên kết với `NewsArticleID` vừa xóa.
- Tương tự, xóa tag: kiểm tra `NewsTag` không còn liên kết với `TagID` vừa xóa.

## Screenshots / Sample outputs
Mình chưa có ảnh chụp màn hình sẵn trong repository. Dưới đây là ví dụ đầu ra console khi chạy app:

```
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

Bạn có thể chụp màn hình trang chủ, trang quản trị bài viết/tag và thêm vào thư mục `docs/screenshots` nếu muốn.

## Thực hành nhanh (PowerShell)
- Build và run:

```powershell
cd D:\FPT\.Net\FUNewsManagement
dotnet build FUNewsManagement.sln
dotnet run --project FUNewsManagement\FUNewsManagement.csproj
```

- Chạy migration (nếu có):

```powershell
cd D:\FPT\.Net\FUNewsManagement\DataAccessLayer
dotnet ef database update --project DataAccessLayer.csproj --startup-project ..\FUNewsManagement\FUNewsManagement.csproj
```

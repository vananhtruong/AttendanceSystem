# Khắc phục vấn đề kết nối giữa Razor Page và API

## Vấn đề đã được khắc phục:

### 1. **URL API không đúng**

- **Vấn đề**: JavaScript trong `WebRazor/Pages/Employee/Index.cshtml` sử dụng URL localhost
- **Khắc phục**: Cập nhật URL thành Azure API endpoint
- **File**: `WebRazor/Pages/Employee/Index.cshtml`

### 2. **CORS Policy chưa đúng**

- **Vấn đề**: CORS policy trong WebAPI không cho phép domain Azure của Razor Page
- **Khắc phục**: Thêm domain Azure vào CORS policy
- **File**: `WebAPI/Program.cs`

### 3. **Thiếu quản lý token nhất quán**

- **Vấn đề**: Token được lưu trữ và sử dụng không nhất quán
- **Khắc phục**: Tạo AuthHelper và ApiHelper để quản lý token
- **Files**:
  - `WebRazor/wwwroot/js/auth-helper.js`
  - `WebRazor/wwwroot/js/api-helper.js`

### 4. **Thiếu service layer cho API calls**

- **Vấn đề**: Các trang Razor gọi API trực tiếp qua HttpClientFactory
- **Khắc phục**: Tạo ApiService để quản lý API calls
- **File**: `WebRazor/Services/ApiService.cs`

### 5. **Middleware xử lý token**

- **Vấn đề**: Token không được tự động thêm vào header
- **Khắc phục**: Tạo TokenMiddleware để xử lý token từ session
- **File**: `WebRazor/Middleware/TokenMiddleware.cs`

## Các file đã được tạo/sửa đổi:

### Files mới:

1. `WebRazor/Services/ApiService.cs` - Service để quản lý API calls
2. `WebRazor/Middleware/TokenMiddleware.cs` - Middleware xử lý token
3. `WebRazor/wwwroot/js/auth-helper.js` - JavaScript helper cho authentication
4. `WebRazor/wwwroot/js/api-helper.js` - JavaScript helper cho API calls
5. `WebRazor/Pages/TestApi.cshtml` - Trang test kết nối API
6. `WebRazor/Pages/TestApi.cshtml.cs` - Code-behind cho trang test

### Files đã sửa đổi:

1. `WebRazor/Program.cs` - Thêm service registration và middleware
2. `WebRazor/Pages/Shared/_Layout.cshtml` - Thêm JavaScript helpers
3. `WebRazor/Pages/Account/Login.cshtml.cs` - Sử dụng ApiService
4. `WebRazor/Pages/Employee/Index.cshtml` - Sử dụng API helper
5. `WebAPI/Program.cs` - Cập nhật CORS policy

## Cách test:

### 1. Test kết nối API:

- Truy cập: `https://your-razor-page-domain/TestApi`
- Click vào các endpoint để test kết nối

### 2. Test authentication:

- Đăng nhập vào hệ thống
- Kiểm tra token được lưu trong localStorage
- Test các chức năng cần authentication

### 3. Test các chức năng chính:

- Employee Management
- Attendance
- Work Schedule
- Salary
- Notifications

## Cấu hình cần thiết:

### 1. Appsettings.json (WebRazor):

```json
{
  "ApiBaseUrl": "https://atten-server-e6hcc8epdkedgzdd.canadacentral-01.azurewebsites.net/",
  "Jwt": {
    "Key": "AttendanceSystemSuperSecretKey123!",
    "Issuer": "AttendanceAPI",
    "Audience": "AttendanceClient"
  }
}
```

### 2. CORS Policy (WebAPI):

```csharp
policy.WithOrigins(
    "https://localhost:7192",
    "https://attendance-management-pages.azurewebsites.net",
    "https://attendance-management-apis.azurewebsites.net",
    "https://atten-server-e6hcc8epdkedgzdd.canadacentral-01.azurewebsites.net"
)
```

## Lưu ý quan trọng:

1. **Deploy lại cả WebAPI và WebRazor** sau khi thay đổi
2. **Kiểm tra URL API** trong appsettings.json
3. **Đảm bảo CORS policy** cho phép domain Razor Page
4. **Test authentication** trước khi test các chức năng khác
5. **Kiểm tra console browser** để debug lỗi JavaScript

## Troubleshooting:

### Nếu vẫn không kết nối được:

1. Kiểm tra URL API có đúng không
2. Kiểm tra CORS policy có cho phép domain không
3. Kiểm tra token có được lưu và gửi đúng không
4. Kiểm tra console browser có lỗi gì không
5. Test trực tiếp API endpoint bằng Postman

### Nếu có lỗi authentication:

1. Kiểm tra JWT key, issuer, audience có khớp giữa WebAPI và WebRazor
2. Kiểm tra token có được tạo đúng không
3. Kiểm tra token có được gửi trong header Authorization không

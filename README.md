# Hệ Thống Điểm Danh Nhân Viên

## Tổng quan

Hệ thống điểm danh nhân viên sử dụng nhận diện khuôn mặt với công nghệ DlibDotNet và .NET 8.0. Hệ thống được xây dựng theo kiến trúc 3-layer với Entity Framework Core.

## Cấu trúc dự án

### 1. BusinessObject

- **Models**: Chứa các entity classes (User, AttendanceRecord, RefreshToken, CorrectionRequest)
- **DTOs**: Data Transfer Objects cho API responses
- **MyDbContext**: Entity Framework DbContext

### 2. DataAccessLayer

- **UserDAO**: Data Access Object cho User entity
- **AttendanceRecordDAO**: Data Access Object cho AttendanceRecord entity

### 3. Repository

- **IUserRepository & UserRepository**: Repository pattern cho User
- **IAttendanceRecordRepository & AttendanceRecordRepository**: Repository pattern cho AttendanceRecord

### 4. WebAPI

- **Controllers**: API endpoints
- **Services**: Business logic services
- **DTOs**: Request/Response DTOs cho WebAPI

## Các tính năng chính

### 1. Authentication & Authorization

- **Register**: Đăng ký tài khoản mới
- **Login**: Đăng nhập với JWT token
- **Refresh Token**: Làm mới access token
- **Role-based Authorization**: Phân quyền Admin/Employee

### 2. Face Recognition

- **Check-in**: Điểm danh bằng nhận diện khuôn mặt
- **Register Face**: Đăng ký hình ảnh khuôn mặt cho user hiện có
- **Register My Face**: Đăng ký hình ảnh khuôn mặt cho chính mình
- **Face Status**: Kiểm tra trạng thái đăng ký face
- **Remove Face**: Xóa dữ liệu khuôn mặt

### 3. Attendance Management

- **View My Attendance**: Xem lịch sử điểm danh của bản thân
- **View All Attendance** (Admin): Xem tất cả điểm danh
- **View User Attendance** (Admin): Xem điểm danh của user cụ thể

### 4. User Management

- **Get All Users** (Admin): Lấy danh sách tất cả users
- **Get User by ID** (Admin): Lấy thông tin user theo ID

## API Endpoints

### Authentication

```
POST /api/account/register
POST /api/account/login
POST /api/account/refresh
GET /api/account/me
GET /api/account/admin-area
```

### Face Recognition

```
POST /api/faceattendance/checkin
POST /api/faceattendance/register-face
POST /api/faceattendance/register-my-face
GET /api/faceattendance/face-status
DELETE /api/faceattendance/remove-my-face
DELETE /api/faceattendance/remove-face/{userId} (Admin only)
```

### Attendance

```
GET /api/attendance/my-attendance
GET /api/attendance/all
GET /api/attendance/user/{userId}
```

### User Management

```
GET /api/user
GET /api/user/{id}
```

## Cài đặt và chạy

### 1. Yêu cầu hệ thống

- .NET 8.0 SDK
- SQL Server
- Windows OS (cho face recognition)

### 2. Cấu hình database

Cập nhật connection string trong `WebAPI/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(local);Database=AttendanceSystem;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### 3. Tạo database

```bash
cd WebAPI
dotnet ef database update
```

### 4. Chạy ứng dụng

```bash
cd WebAPI
dotnet run
```

### 5. Truy cập Swagger UI

Mở trình duyệt và truy cập: `https://localhost:7001/swagger`

## Các vấn đề đã được sửa

### 1. Cấu trúc và Dependencies

- ✅ Sửa namespace lồng nhau trong UserDTO
- ✅ Tách DTOs phù hợp cho từng layer
- ✅ Thêm validation cho DTOs
- ✅ Sửa import statements sai

### 2. Authentication & Security

- ✅ Thay thế SHA256 bằng BCrypt cho password hashing
- ✅ Sửa logic refresh token
- ✅ Thêm UserId cho RefreshToken

### 3. Database & Entity Framework

- ✅ Khởi tạo collections trong User model
- ✅ Sửa UpdateAsync methods
- ✅ Thêm methods cho date range queries
- ✅ Cải thiện relationship mappings

### 4. Face Recognition

- ✅ Sửa logic attendance recording
- ✅ Thêm kiểm tra attendance đã tồn tại
- ✅ Cải thiện error handling

### 5. API Controllers

- ✅ Thêm AttendanceController
- ✅ Thêm UserController
- ✅ Sửa response formats
- ✅ Thêm proper authorization

### 6. Business Logic

- ✅ Cải thiện attendance workflow (check-in/check-out)
- ✅ Thêm date range filtering
- ✅ Sửa face recognition service

## Lưu ý quan trọng

1. **Face Recognition Models**: Cần tải các model files của DlibDotNet và đặt trong thư mục `WebAPI/Models/`
2. **Face Registration Process**:
   - User đăng ký tài khoản thông thường trước
   - Sau đó đăng ký hình ảnh khuôn mặt để sử dụng face recognition
   - Có thể kiểm tra trạng thái đăng ký face và xóa face data khi cần
3. **Windows Only**: Face recognition chỉ hoạt động trên Windows do sử dụng System.Drawing
4. **Database**: Đảm bảo SQL Server đang chạy và connection string đúng

## Cải tiến có thể thực hiện

1. Thêm logging chi tiết
2. Implement caching cho face recognition
3. Thêm email notifications
4. Implement real-time attendance tracking
5. Thêm reporting và analytics
6. Implement mobile app support

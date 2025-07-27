# Employee Profile Page Features

## Overview
Trang Profile cho Employee được tạo với các tính năng sau:

### 1. Thông tin cá nhân
- Hiển thị thông tin cá nhân của employee: Full Name, Email, Role
- Thông tin được lấy từ API `/api/account/me`
- Hiển thị trạng thái Face ID (đã đăng ký hay chưa)

### 2. Đổi mật khẩu
- Form đổi mật khẩu với 3 trường:
  - Current Password: Mật khẩu hiện tại
  - New Password: Mật khẩu mới
  - Confirm New Password: Xác nhận mật khẩu mới
- Validation: Mật khẩu mới phải khớp với xác nhận
- API endpoint: `/api/account/change-password`

### 3. Face ID Registration
- **Kiểm tra trạng thái Face ID**: Hiển thị đã đăng ký hay chưa
- **Đăng ký Face ID**: 
  - Nút "Register Face ID" mở camera
  - Chụp ảnh và xác nhận
  - Gửi ảnh lên API `/api/faceattendance/register-my-face`
- **Xóa Face ID**: 
  - Nút "Remove Face ID" để xóa dữ liệu face
  - API endpoint: `/api/faceattendance/remove-my-face`

### 4. Camera Features
- **Mở camera**: Sử dụng WebRTC để truy cập camera
- **Chụp ảnh**: Capture frame từ video stream
- **Xem lại ảnh**: Hiển thị ảnh đã chụp để xác nhận
- **Chụp lại**: Nút "Retake" để chụp lại ảnh
- **Xác nhận**: Nút "Confirm & Register" để đăng ký face ID

## API Endpoints Used

### 1. Get Current User
```
GET /api/account/me
Authorization: Bearer {token}
Response: ApiResponseDto<UserDTO>
```

### 2. Change Password
```
POST /api/account/change-password
Authorization: Bearer {token}
Body: ChangePasswordRequest
Response: ApiResponseDto<string>
```

### 3. Check Face Status
```
GET /api/faceattendance/face-status
Authorization: Bearer {token}
Response: ApiResponseDto<{hasFaceRegistered: boolean}>
```

### 4. Register Face ID
```
POST /api/faceattendance/register-my-face
Authorization: Bearer {token}
Body: FormData with faceImage
Response: ApiResponseDto<string>
```

### 5. Remove Face ID
```
DELETE /api/faceattendance/remove-my-face
Authorization: Bearer {token}
Response: ApiResponseDto<string>
```

## File Structure

### Frontend Files
- `WebRazor/Pages/Account/Profile.cshtml` - Giao diện trang Profile
- `WebRazor/Pages/Account/Profile.cshtml.cs` - Code-behind
- `WebRazor/wwwroot/css/site.css` - CSS styles cho Profile

### Backend Files
- `WebAPI/Controllers/AccountController.cs` - API endpoints cho user management
- `WebAPI/Controllers/FaceAttendanceController.cs` - API endpoints cho face recognition
- `BusinessObject/DTOs/ChangePasswordRequest.cs` - DTO cho đổi mật khẩu
- `BusinessObject/DTOs/UserDTO.cs` - DTO cho user information

## Security Features
- **Authentication**: Tất cả API endpoints yêu cầu JWT token
- **Authorization**: Kiểm tra role và user permissions
- **Password Validation**: Verify current password trước khi đổi
- **Face Data Security**: Face descriptors được mã hóa và lưu an toàn

## UI/UX Features
- **Responsive Design**: Tương thích với mobile và desktop
- **Modern UI**: Sử dụng Bootstrap 5 và custom CSS
- **Real-time Feedback**: Alert messages cho mọi actions
- **Loading States**: Hiển thị trạng thái loading khi cần
- **Error Handling**: Xử lý lỗi và hiển thị thông báo phù hợp

## Browser Compatibility
- **Camera Access**: Yêu cầu HTTPS hoặc localhost
- **WebRTC**: Hỗ trợ trên các browser hiện đại
- **File API**: Hỗ trợ upload và xử lý ảnh

## Usage Instructions
1. Employee đăng nhập vào hệ thống
2. Click vào menu "Profile" trong sidebar
3. Xem thông tin cá nhân và trạng thái Face ID
4. Đổi mật khẩu nếu cần
5. Đăng ký Face ID bằng cách:
   - Click "Register Face ID"
   - Cho phép truy cập camera
   - Chụp ảnh và xác nhận
   - Click "Confirm & Register"
6. Xóa Face ID nếu cần bằng nút "Remove Face ID" 
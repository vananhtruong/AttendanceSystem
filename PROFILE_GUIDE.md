# Profile Page Guide

## 🎯 Tính năng đã hoàn thành:

### ✅ 1. **Edit Profile (Chỉnh sửa thông tin cá nhân)**
- Form chỉnh sửa Full Name và Email
- Validation: Email không được trùng với user khác
- API endpoint: `PUT /api/account/update-profile`
- Real-time update: Sau khi update thành công, thông tin hiển thị sẽ được cập nhật

### ✅ 2. **Change Password (Đổi mật khẩu)**
- Form đổi mật khẩu với 3 trường:
  - Current Password: Mật khẩu hiện tại
  - New Password: Mật khẩu mới
  - Confirm New Password: Xác nhận mật khẩu mới
- Validation: Mật khẩu mới phải khớp với xác nhận
- API endpoint: `POST /api/account/change-password`

### ✅ 3. **Face ID Registration (Giao diện)**
- Hiển thị trạng thái Face ID (Not Registered)
- Nút "Register Face ID" mở camera modal
- Camera modal với các chức năng:
  - Chụp ảnh
  - Xem lại ảnh
  - Chụp lại
  - Xác nhận đăng ký
- Nút "Remove Face ID" để xóa
- **Lưu ý**: Chỉ có giao diện, API sẽ được implement sau

## 🚀 Cách sử dụng:

### 1. **Khởi động ứng dụng:**
```bash
# Terminal 1: Khởi động WebAPI
cd WebAPI
dotnet run --urls "https://localhost:7192;http://localhost:7192"

# Terminal 2: Khởi động WebRazor
cd WebRazor
dotnet run
```

### 2. **Truy cập Profile page:**
- Đăng nhập vào WebRazor
- Click vào menu "Profile" trong sidebar
- Hoặc truy cập trực tiếp: `https://localhost:7192/Account/Profile`

### 3. **Edit Profile:**
- Điền thông tin mới vào form "Edit Profile"
- Click "Update Profile"
- Thông tin sẽ được cập nhật và hiển thị ngay lập tức

### 4. **Change Password:**
- Điền mật khẩu hiện tại
- Điền mật khẩu mới và xác nhận
- Click "Change Password"
- Form sẽ được reset sau khi thành công

### 5. **Face ID (Giao diện):**
- Click "Register Face ID" để mở camera
- Cho phép truy cập camera
- Chụp ảnh và xác nhận
- Hiện tại chỉ hiển thị thông báo "Feature will be implemented soon"

## 📁 Files đã tạo/cập nhật:

### Backend:
- `BusinessObject/DTOs/UpdateProfileRequest.cs` - DTO cho update profile
- `WebAPI/Controllers/AccountController.cs` - Thêm endpoint update-profile
- `WebAPI/Program.cs` - Cấu hình services

### Frontend:
- `WebRazor/Pages/Account/Profile.cshtml` - Giao diện Profile page
- `WebRazor/Pages/Account/Profile.cshtml.cs` - Code-behind
- `WebRazor/wwwroot/css/site.css` - CSS styles

### Test Files:
- `test_profile.html` - File test API endpoints
- `PROFILE_GUIDE.md` - Hướng dẫn này

## 🔧 API Endpoints:

### 1. Get Current User
```
GET /api/account/me
Authorization: Bearer {token}
Response: ApiResponseDto<UserDTO>
```

### 2. Update Profile
```
PUT /api/account/update-profile
Authorization: Bearer {token}
Body: UpdateProfileRequest
Response: ApiResponseDto<string>
```

### 3. Change Password
```
POST /api/account/change-password
Authorization: Bearer {token}
Body: ChangePasswordRequest
Response: ApiResponseDto<string>
```

## 🎨 Giao diện:

- **Modern UI** với Bootstrap 5
- **Responsive design** cho mobile và desktop
- **Real-time feedback** với alert messages
- **Loading states** và error handling
- **Gradient colors** và hover effects

## 🔒 Security:

- **JWT Authentication** cho tất cả API calls
- **Input validation** cho email và password
- **Email uniqueness check** khi update profile
- **Password verification** khi đổi mật khẩu

## 🚧 Face ID Implementation (Sau này):

- **Face Recognition API** sẽ được implement sau
- **Camera integration** đã sẵn sàng
- **Image processing** và face detection
- **Database storage** cho face descriptors

## 🧪 Testing:

1. **Test API endpoints:**
   - Mở `test_profile.html` trong browser
   - Click các nút test để kiểm tra API

2. **Test Profile page:**
   - Đăng nhập và vào Profile page
   - Thử edit profile và change password
   - Kiểm tra Face ID giao diện

## 📝 Notes:

- Face ID chỉ có giao diện, chưa có API thực sự
- Token được hardcode cho testing
- Có thể cần restart WebAPI nếu có lỗi build
- Console logs sẽ hiển thị debug information 
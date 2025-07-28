# Database Connection Fix Guide

## Vấn đề hiện tại:

- **Status**: 500 - Database connection failed
- **Nguyên nhân**: Không thể kết nối đến Monster ASP.NET database server

## Các bước khắc phục:

### **Bước 1: Deploy lại WebAPI**

Sau khi sửa đổi connection string, deploy lại WebAPI với:

- URL encoded password: `F%3Fz7_3HqmZ%405` (thay vì `F?z7_3HqmZ@5`)
- Thêm `Connection Timeout=30`
- Thêm `TrustServerCertificate=True`

### **Bước 2: Test theo thứ tự**

Truy cập: `https://your-razor-page-domain/TestApi`

**Thứ tự test:**

1. **Test Basic Connection** - Kiểm tra API hoạt động
2. **Test Connection Info** - Kiểm tra connection string
3. **Test Direct Connection** - Test kết nối trực tiếp
4. **Test Database Connection** - Test qua Entity Framework
5. **Test Database Schema** - Kiểm tra tables

### **Bước 3: Phân tích kết quả**

#### **Nếu Test Direct Connection thành công:**

- ✅ Kết nối database server OK
- ❌ Vấn đề với Entity Framework configuration

#### **Nếu Test Direct Connection thất bại:**

- ❌ Vấn đề với connection string hoặc server
- ❌ Kiểm tra credentials hoặc network

### **Bước 4: Các nguyên nhân có thể**

#### **1. Password Encoding**

**Vấn đề**: Password có ký tự đặc biệt `?` và `@`
**Khắc phục**: URL encode password

```
F?z7_3HqmZ@5 → F%3Fz7_3HqmZ%405
```

#### **2. Connection Timeout**

**Vấn đề**: Kết nối timeout quá lâu
**Khắc phục**: Thêm `Connection Timeout=30`

#### **3. SSL Certificate**

**Vấn đề**: SSL certificate không hợp lệ
**Khắc phục**: Thêm `TrustServerCertificate=True`

#### **4. Server Accessibility**

**Vấn đề**: Server không accessible từ Azure
**Khắc phục**: Kiểm tra firewall và network

### **Bước 5: Alternative Connection Strings**

Nếu vẫn không hoạt động, thử các connection string khác:

#### **Option 1: Không encode password**

```json
"DefaultConnection": "Server=db24241.databaseasp.net; Database=db24241; User Id=db24241; Password=\"F?z7_3HqmZ@5\"; Encrypt=False; MultipleActiveResultSets=True; TrustServerCertificate=True; Connection Timeout=30;"
```

#### **Option 2: Sử dụng connection string builder**

```json
"DefaultConnection": "Server=db24241.databaseasp.net; Database=db24241; User Id=db24241; Password=F?z7_3HqmZ@5; Encrypt=False; MultipleActiveResultSets=True; TrustServerCertificate=True; Connection Timeout=30;"
```

#### **Option 3: Thêm các tham số khác**

```json
"DefaultConnection": "Server=db24241.databaseasp.net; Database=db24241; User Id=db24241; Password=F%3Fz7_3HqmZ%405; Encrypt=False; MultipleActiveResultSets=True; TrustServerCertificate=True; Connection Timeout=30; Command Timeout=30;"
```

### **Bước 6: Test với SQL Server Management Studio**

Nếu có thể, test connection trực tiếp:

```
Server: db24241.databaseasp.net
Database: db24241
Username: db24241
Password: F?z7_3HqmZ@5
```

### **Bước 7: Kiểm tra Monster ASP.NET Dashboard**

1. Đăng nhập vào Monster ASP.NET dashboard
2. Kiểm tra database status
3. Kiểm tra connection limits
4. Kiểm tra firewall settings

### **Bước 8: Debug chi tiết**

#### **Kiểm tra logs:**

- WebAPI logs trong Azure
- Database server logs
- Application logs

#### **Kiểm tra network:**

- Ping database server
- Telnet database port (1433)
- Check firewall rules

### **Bước 9: Fallback Solutions**

#### **Nếu Monster ASP.NET có vấn đề:**

1. Tạo database mới trên Azure SQL
2. Sử dụng local database để test
3. Sử dụng SQLite cho development

#### **Nếu connection string có vấn đề:**

1. Sử dụng connection string builder
2. Kiểm tra encoding
3. Test từng tham số một

### **Bước 10: Final Test**

Sau khi khắc phục:

1. Test direct connection
2. Test database connection
3. Test login với test user
4. Test các chức năng chính

## Liên hệ hỗ trợ

Nếu vẫn gặp vấn đề, cung cấp:

1. Kết quả test từ tất cả endpoints
2. Error messages chi tiết
3. Monster ASP.NET dashboard status
4. Network connectivity test results

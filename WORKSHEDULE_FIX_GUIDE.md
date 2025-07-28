# Hướng dẫn khắc phục lỗi FOREIGN KEY constraint cho WorkSchedule

## Nguyên nhân lỗi

Lỗi `FK_WorkSchedules_WorkShifts_WorkShiftId` xảy ra khi:

1. Tạo WorkSchedule với WorkShiftId không tồn tại trong bảng WorkShifts
2. Tạo WorkSchedule với UserId không tồn tại trong bảng Users
3. Thiếu cấu hình foreign key relationship trong Entity Framework

## Các bước khắc phục

### 1. Chạy Script SQL để kiểm tra và sửa dữ liệu

```sql
-- Chạy file DATABASE_FIX_SCRIPT.sql trong SQL Server Management Studio
-- Hoặc chạy từng phần trong script để:
-- - Kiểm tra dữ liệu hiện có
-- - Xóa dữ liệu không hợp lệ (nếu cần)
-- - Tạo WorkShifts mặc định
```

### 2. Cập nhật Database Schema

```bash
# Trong thư mục WebAPI
dotnet ef migrations add AddWorkScheduleForeignKeys
dotnet ef database update
```

### 3. Kiểm tra và chạy ứng dụng

```bash
# Build và chạy WebAPI
dotnet build
dotnet run

# Build và chạy WebRazor
cd ../WebRazor
dotnet build
dotnet run
```

## Các cải tiến đã thực hiện

### Backend (WebAPI)

1. **Thêm validation trong WorkScheduleController**:

   - Kiểm tra User tồn tại trước khi tạo WorkSchedule
   - Kiểm tra WorkShift tồn tại trước khi tạo WorkSchedule
   - Kiểm tra trùng lặp WorkSchedule

2. **Cấu hình Foreign Key trong MyDbContext**:

   - Thêm relationship giữa WorkSchedule và User
   - Thêm relationship giữa WorkSchedule và WorkShift
   - Cấu hình DeleteBehavior phù hợp

3. **Thêm Data Validation trong DTOs**:
   - Required attributes cho các trường bắt buộc
   - Range validation cho ID fields
   - String length validation

### Frontend (WebRazor)

1. **Load WorkShifts động**:

   - Thay thế hardcode options bằng API call
   - Hiển thị thông tin chi tiết ca làm việc

2. **Cải thiện Error Handling**:

   - Hiển thị thông báo lỗi chi tiết
   - Validation client-side
   - Logging lỗi để debug

3. **Cải thiện UX**:
   - Loading states
   - Thông báo kết quả chi tiết
   - Validation trước khi submit

## Kiểm tra sau khi khắc phục

### 1. Kiểm tra Database

```sql
-- Kiểm tra foreign key constraints
SELECT
    fk.name as ConstraintName,
    OBJECT_NAME(fk.parent_object_id) as TableName,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) as ColumnName,
    OBJECT_NAME(fk.referenced_object_id) as ReferencedTableName,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) as ReferencedColumnName
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) = 'WorkSchedules';
```

### 2. Kiểm tra API

```bash
# Test tạo WorkSchedule với dữ liệu hợp lệ
curl -X POST "https://localhost:7117/api/WorkSchedule" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 1,
    "workShiftId": 1,
    "workDate": "2025-01-15",
    "status": "NotYet"
  }'

# Test tạo WorkSchedule với WorkShiftId không tồn tại (phải trả về lỗi)
curl -X POST "https://localhost:7117/api/WorkSchedule" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 1,
    "workShiftId": 999,
    "workDate": "2025-01-15",
    "status": "NotYet"
  }'
```

### 3. Kiểm tra Frontend

- Mở trang WorkSchedule Management
- Kiểm tra dropdown WorkShift load đúng dữ liệu
- Test tạo WorkSchedule với các trường hợp khác nhau
- Kiểm tra thông báo lỗi hiển thị đúng

## Lưu ý quan trọng

1. **Backup Database**: Luôn backup database trước khi chạy migration
2. **Test Environment**: Test trên môi trường development trước khi deploy production
3. **Data Integrity**: Đảm bảo dữ liệu hiện có không bị ảnh hưởng
4. **Performance**: Monitor performance sau khi thêm foreign key constraints

## Troubleshooting

### Lỗi Migration

```bash
# Nếu migration bị lỗi, có thể cần:
dotnet ef migrations remove
dotnet ef migrations add AddWorkScheduleForeignKeys
```

### Lỗi Foreign Key khi chạy

```sql
-- Kiểm tra dữ liệu không hợp lệ
SELECT ws.* FROM WorkSchedules ws
LEFT JOIN WorkShifts wsh ON ws.WorkShiftId = wsh.Id
WHERE wsh.Id IS NULL;
```

### Lỗi Frontend

- Kiểm tra Console browser để xem lỗi JavaScript
- Kiểm tra Network tab để xem API calls
- Đảm bảo API endpoints trả về đúng format

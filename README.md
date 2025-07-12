# ĐỀ TÀI: ỨNG DỤNG CHAT REAL-TIME

**Môn học:** LẬP TRÌNH MẠNG CĂN BẢN  
**Giảng viên:** ThS. Trần Hồng Nghi

## Thành viên nhóm

- **Huỳnh Minh Quí** - 23521298
- **Huỳnh Hoàng Huy** - 23520606
- **Huỳnh Anh Khôi** - 23520764

---

## Mô tả đề tài

Đề tài xây dựng một ứng dụng chat real-time với các tính năng:

- Chat cá nhân (text, emoji, hình ảnh, file đính kèm).
- Chat nhóm (text, emoji, hình ảnh, file đính kèm).
- Gọi thoại (voice call).
- Gọi video (video call).
- Quản lý danh sách bạn bè.
- Quản lý nhóm chat, phân quyền quản trị viên.
- Xác thực người dùng qua email.

Hệ thống sử dụng SignalR để đảm bảo khả năng giao tiếp thời gian thực giữa client và server. Các dữ liệu đa phương tiện (hình ảnh, file) được lưu trữ trên dịch vụ đám mây Amazon S3, giúp tối ưu lưu trữ và băng thông server.

---

## Chức năng chính

- Đăng ký, đăng nhập tài khoản người dùng.
- Xác thực email với mã OTP (sử dụng MailKit).
- Tìm kiếm, kết bạn, quản lý danh sách bạn bè.
- Chat cá nhân real-time (text, emoji, hình ảnh, file).
- Chat nhóm real-time, quản lý thành viên, phân quyền nhóm.
- Gọi thoại giữa 2 người.
- Gọi video giữa 2 người.
- Lưu trữ lịch sử tin nhắn vào cơ sở dữ liệu.
- Lưu trữ hình ảnh và file trên Amazon S3.

---

## Công nghệ sử dụng

### Ngôn ngữ lập trình C#

C# (C-sharp) là một ngôn ngữ lập trình hiện đại, đa mục đích do Microsoft phát triển, được thiết kế chủ yếu để xây dựng các ứng dụng chạy trên nền tảng Windows. Ngoài ra, C# cũng được ứng dụng rộng rãi trong phát triển phần mềm web, ứng dụng di động và các hệ thống đa nền tảng thông qua .NET.

Một số đặc điểm nổi bật:

- Khả năng đa nền tảng (Windows, macOS, Linux).
- Tính an toàn cao, kiểm tra kiểu dữ liệu chặt chẽ.
- Hỗ trợ lập trình hướng đối tượng toàn diện.

---

### Windows Forms (WinForms)

Windows Forms (WinForms) là thư viện GUI truyền thống của .NET, dùng để xây dựng giao diện ứng dụng Windows. Các thành phần cơ bản:

- **Form**: cửa sổ chính hoặc các cửa sổ con.
- **Control**: Button, TextBox, Label, Panel,… dùng để hiển thị và nhận tương tác người dùng.

---

### SignalR

SignalR là thư viện mã nguồn mở của Microsoft, cho phép xây dựng ứng dụng có khả năng giao tiếp thời gian thực (real-time). Hỗ trợ nhiều cơ chế kết nối như WebSocket, Server-Sent Events, Long Polling, giúp hoạt động ổn định trên nhiều trình duyệt và môi trường.

Trong ứng dụng chat, SignalR đảm nhiệm:

- Gửi và nhận tin nhắn real-time.
- Truyền dữ liệu audio, video trong voice call và video call.
- Đồng bộ các thay đổi nhóm chat (thành viên, tên nhóm, ảnh đại diện).

---

### MailKit

MailKit là thư viện .NET hiện đại, hỗ trợ các giao thức gửi và nhận email như SMTP, POP3, IMAP. Trong dự án:

- Gửi email xác thực tài khoản khi đăng ký.
- Tăng tính bảo mật cho quá trình đăng ký người dùng.

---

### Microsoft SQL Server (MSSQL)

Microsoft SQL Server là hệ quản trị cơ sở dữ liệu mạnh mẽ của Microsoft. Trong hệ thống chat:

- Lưu trữ thông tin người dùng.
- Lưu lịch sử tin nhắn (cá nhân và nhóm).
- Lưu thông tin nhóm chat, danh sách bạn bè.

---

### Amazon S3

Amazon S3 (Simple Storage Service) là dịch vụ lưu trữ đám mây của Amazon:

- Lưu trữ hình ảnh, file gửi trong chat.
- Tối ưu dung lượng lưu trữ server.
- Đảm bảo tính bền vững và tốc độ truy xuất dữ liệu.

---

### XAML

XAML (eXtensible Application Markup Language) là ngôn ngữ đánh dấu dùng để xây dựng giao diện người dùng (UI) trong các ứng dụng .NET (WPF, UWP, MAUI). Trong dự án:

- Thiết kế UI các cửa sổ: chat, danh sách bạn bè, voice call, video call.
- Hỗ trợ binding dữ liệu giúp cập nhật giao diện theo thời gian thực.

---

### NAudio

NAudio là thư viện .NET xử lý âm thanh:

- Thu âm từ micro.
- Ghi âm, phát lại âm thanh trong voice call.

---

### AForge.NET

AForge.NET là thư viện xử lý hình ảnh, video:

- Kết nối camera.
- Lấy frame video.
- Xử lý ảnh trước khi gửi đi trong video call (resize, nén ảnh).

---

## Cách chạy ứng dụng

### 1. Cài đặt môi trường

- Visual Studio 2022 (hoặc mới hơn)
- .NET 6 trở lên
- SQL Server
- Tài khoản AWS để sử dụng S3

### 2. Khởi chạy server

- Chạy project **ChatApp.Server**.
- Kiểm tra các hub SignalR đã lắng nghe đúng địa chỉ:
  - http://localhost:5000/socket/chat-group
  - http://localhost:5000/socket/voice-call
  - http://localhost:5000/socket/video-call

### 3. Chạy client

- Chạy project **ChatApp.Client** (WinForms hoặc WPF).
- Đăng ký tài khoản.
- Đăng nhập và trải nghiệm các tính năng chat, call.

---

## Ghi chú

- Để sử dụng AWS S3, cần cấu hình các thông số:
  - AWS Access Key
  - AWS Secret Key
  - Bucket Name

- MailKit cần cấu hình thông tin SMTP server (ví dụ Gmail).

---


# Nhiệm vụ 1: Phân tích vi phạm SOLID trong `ClubManager.cs`


## Bảng vi phạm (≥ 4, mỗi vi phạm: dòng nào – nguyên lý nào – hậu quả gì)

| # | Vị trí (dòng) | Dấu hiệu trong code | Nguyên lý | Hậu quả | Cách sửa trong bài giải |
|---|---|---|---|---|---|
| 1 | `CreateClub`, dòng 35–64 (validate 37–41, lưu 44–45, log 48, báo tin 52–63) | Một method vừa kiểm tra dữ liệu, vừa lưu, vừa ghi file log, vừa gửi thông báo | **S – SRP** | Có ≥ 4 lý do để sửa method (luật tên, nơi lưu, định dạng log, kênh báo tin); sửa việc này dễ làm hỏng việc kia; không test riêng từng việc được | `Club.Create` (kiểm tra), `IClubRepository` (lưu), `IAppLogger` (log), `INotificationServiceFactory` (báo tin); `ClubService` chỉ điều phối |
| 2 | `CreateClub`, dòng 52–63 | Chuỗi `if (Contains("@gmail.com")) … else if ("@yahoo.com") … else …` | **O – OCP** | Thêm SMS/Slack hay nhà cung cấp mới phải mở method ra sửa, dễ làm hỏng nhánh cũ | `NotificationServiceFactory` + mỗi kênh một class cài `INotificationService` |
| 3 | `AddMemberToClub`, dòng 82–83 | Gửi cứng "Welcome email" và "Slack notification" bằng `Console.WriteLine` ngay trong nghiệp vụ | **O – OCP** + **S – SRP** | Thêm/bớt kênh phải sửa nghiệp vụ thêm thành viên | `ClubService.AddMember` lấy kênh từ factory |
| 4 | Dòng 48, 79, 94, 111 (`File.AppendAllText`) và dòng 54, 58, 62, 82, 83, 97, 108 (`Console.WriteLine`) | Logic nghiệp vụ gọi thẳng `File` và `Console` | **D – DIP** | Logic dính chặt vào I/O: muốn đổi nơi ghi log phải sửa class; test phải tạo file thật và in ra màn hình, không dùng bản giả được | `IAppLogger` (`ConsoleLogger`, `FileLogger`, `CompositeLogger`); test dùng `FakeLogger` |
| 5 | Dòng 30–32 (`_clubs`, `_members`, `_logFilePath`) và dòng 44, 75, 93 (`new Club`, `new Member`, `new Event` trong method) | Class tự giữ kho dữ liệu, đường dẫn log cứng và tự tạo đối tượng | **D – DIP** (và **S – SRP**) | Đổi nơi lưu (CSDL) hay viết test đều phải sửa `ClubManager`; kho dữ liệu lẫn với nghiệp vụ | `IClubRepository`, `IMemberRepository`, `IEventRepository` (`InMemory…`); **chỉ `Program.cs` dùng `new` cho class cụ thể** |
| 6 | `CreateEvent`, dòng 88–99 (log 94, vòng gửi mail 96–97) | Tạo sự kiện nhưng cũng tự ghi log và gửi email cho từng thành viên | **S – SRP** + **O – OCP** | Muốn báo thêm cho bên khác (SMS, Slack) phải sửa `CreateEvent` | **Observer**: `IEventObserver`, `LogNotifier`, `EmailNotifier`; `EventService` chỉ gọi `OnEventCreated` |
| 7 | `PrintClubReport`, dòng 103–113 | Định dạng báo cáo viết cứng + ghi file ngay trong lúc in | **O – OCP** + **D – DIP** | Muốn xuất CSV/JSON phải sửa method | **Strategy**: `IReportStrategy` (`ConsoleReportStrategy`, `CsvReportStrategy`) + `ReportService.Generate` |
| 8 | `GetAllClubs`, dòng 116 (`=> _clubs`) | Trả nguyên danh sách nội bộ ra ngoài | Che giấu dữ liệu (encapsulation); liên quan **I/L** vì không có hợp đồng đọc/ghi | Bên ngoài `Add/Remove` thẳng vào danh sách, phá luật nghiệp vụ | `IClubReader` (chỉ đọc) tách khỏi `IClubRepository`; repository trả bản sao `ToList()` |
| 9 | Dòng 40 và 72 (số `100`, `50` viết cứng) | Luật nghiệp vụ nằm trong `ClubManager`, trùng với `Club.MaxNameLength/MaxMembers` | **S – SRP** (luật không ở Domain) | Luật rải rác, sửa một nơi quên nơi khác | Chuyển vào `Club.Create` và `Club.HasRoom` |
| 10 | `CreateEvent`, dòng 93–98 | Tạo `Event` nhưng **không lưu** vào đâu (chỉ `return`) | Lỗi chức năng (không phải SOLID) | Không thể đăng ký/check-in vào sự kiện đã tạo | `IEventRepository` + `EventService` lưu sự kiện |

## Kết luận ngắn

`ClubManager` là **God class**: ôm kiểm tra dữ liệu, lưu trữ, ghi log, gửi thông báo và in báo cáo, nên vi phạm đồng thời **SRP, OCP, DIP**, đồng thời lộ danh sách nội bộ (ảnh hưởng ISP/LSP ở tầng kho dữ liệu). Sau refactor: mỗi class một lý do thay đổi, mọi phụ thuộc đi qua interface và được ráp ở `Program.cs`, nên code dễ đọc, dễ mở rộng (thêm kênh/định dạng/observer không sửa code cũ) và dễ kiểm thử (dùng bản giả, không cần File/Console). Chức năng bên ngoài giữ nguyên (đúng nghĩa refactor).

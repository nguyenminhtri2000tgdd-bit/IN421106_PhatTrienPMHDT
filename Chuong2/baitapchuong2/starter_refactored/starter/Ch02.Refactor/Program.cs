// [GT] Program.cs = "composition root": NƠI DUY NHẤT dùng "new" cho class cụ thể rồi ráp mọi thứ lại với nhau.
// [GT] Các class còn lại chỉ biết interface. Chương 3 sẽ giao việc "ráp" này cho DI container của .NET.
Console.OutputEncoding = System.Text.Encoding.UTF8;   // [GT] để in đúng tiếng Việt có dấu

// ---------- Ráp các thành phần ----------
// [GT] Ghi log vừa ra màn hình vừa ra file club-log.txt (giữ hành vi file của code cũ). Đổi nơi ghi log = đổi dòng này, service không đổi.
IAppLogger logger = new CompositeLogger(new ConsoleLogger(), new FileLogger("club-log.txt"));
INotificationServiceFactory notifications = new NotificationServiceFactory();   // [GT] Factory

// [GT] DECORATOR: bọc repository thật bằng LoggingClubRepository. Bỏ new LoggingClubRepository(...) đi thì dòng log [repo] biến mất, service không sửa.
IClubRepository clubRepo = new LoggingClubRepository(new InMemoryClubRepository(), logger);
IMemberRepository memberRepo = new InMemoryMemberRepository();
IEventRepository eventRepo = new InMemoryEventRepository();

IClubService clubService = new ClubService(clubRepo, memberRepo, notifications, logger);
var eventService = new EventService(eventRepo, clubRepo, memberRepo);   // [GT] dùng "var" vì cần gọi Subscribe
var reportService = new ReportService(clubRepo, memberRepo, eventRepo, logger);

static void Section(string title) => Console.WriteLine($"\n== {title} ==");

// ---------- 1. Tạo CLB và thành viên (demo giống Program.cs cũ) ----------
Section("1. Tạo CLB và thành viên");
var club = clubService.CreateClub("Tech Club", "Câu lạc bộ công nghệ", "president@gmail.com");
var memberA = clubService.AddMember(club.Id, "Nguyễn Văn A", "a@student.dthu.edu.vn");
var memberB = clubService.AddMember(club.Id, "Trần Thị B", "b@student.dthu.edu.vn");

// ---------- 2. Observer ----------
Section("2. Observer: tạo sự kiện -> các observer được báo");
// [GT] Đăng ký 2 observer. Muốn thêm SMS: bỏ comment dòng dưới, KHÔNG phải sửa EventService.
eventService.Subscribe(new LogNotifier(logger));
eventService.Subscribe(new EmailNotifier(memberRepo, notifications.Create(NotificationChannel.Email)));
// eventService.Subscribe(new SmsNotifier(memberRepo, notifications.Create(NotificationChannel.Sms)));
var ev = eventService.CreateEvent(club.Id, "Workshop SOLID", new DateTime(2026, 11, 20));

Console.WriteLine($"Đăng ký A: {eventService.Register(ev.Id, memberA.Id)}");        // [GT] true: đăng ký mới
Console.WriteLine($"Đăng ký B: {eventService.Register(ev.Id, memberB.Id)}");
Console.WriteLine($"Đăng ký A lần 2: {eventService.Register(ev.Id, memberA.Id)}");   // [GT] false: trùng, Domain từ chối
eventService.CheckIn(ev.Id, memberA.Id);                                             // [GT] A đã đăng ký nên check-in được

// ---------- 3. Strategy ----------
Section("3. Strategy: báo cáo dạng Console");
Console.WriteLine(reportService.Generate(new ConsoleReportStrategy()));   // [GT] cùng dữ liệu, đổi strategy là đổi định dạng
Section("3. Strategy: báo cáo dạng CSV");
Console.WriteLine(reportService.Generate(new CsvReportStrategy()));

// ---------- 4. Factory ----------
Section("4. Factory: tạo kênh thông báo theo yêu cầu");
foreach (var channel in Enum.GetValues<NotificationChannel>())   // [GT] duyệt Email, Sms, Slack
    notifications.Create(channel).Send("a@student.dthu.edu.vn", $"Thử kênh {channel}");

// ---------- 5. Kiểm tra dữ liệu sai ----------
Section("5. Kiểm tra dữ liệu sai (Domain phải từ chối)");
try { clubService.CreateClub("   ", "tên rỗng", "x@x.vn"); }
catch (ArgumentException ex) { logger.Error($"Từ chối tạo CLB: {ex.Message}"); }

try { eventService.CheckIn(ev.Id, Guid.NewGuid()); }   // [GT] người lạ chưa đăng ký mà check-in
catch (InvalidOperationException ex) { logger.Error($"Từ chối check-in: {ex.Message}"); }

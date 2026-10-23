using System.Text;
using EduHub.Ch01.Domain;

Console.OutputEncoding = Encoding.UTF8;

// "Cơ sở dữ liệu" tạm: Chương 1 chưa có database nên dùng List trong bộ nhớ.
var registrations = new List<Registration>();
var checkIns = new List<CheckIn>();

// ── Hàm tiện ích ────────────────────────────────────────────────
void Title(string text) => Console.WriteLine($"\n=== {text} ===");

// Thử chạy một thao tác; nếu vi phạm luật nghiệp vụ thì in lỗi thay vì dừng chương trình.
void Try(string description, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  [OK]      {description}");
    }
    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
    {
        // ArgumentOutOfRangeException là con của ArgumentException nên cũng được bắt ở đây.
        Console.WriteLine($"  [TỪ CHỐI] {description} → {ex.Message}");
    }
}

// Số phiếu còn hiệu lực của một sự kiện (không tính phiếu đã huỷ).
int ActiveRegistrations(Event ev) =>
    registrations.Count(r => r.EventId == ev.Id && r.Status != RegistrationStatus.Cancelled);

// ── Bước 1: Tạo CLB ─────────────────────────────────────────────
Title("Bước 1: Tạo CLB");
var club = Club.Create("CLB Lập trình DThU", "Sinh hoạt về lập trình và công nghệ phần mềm");
Console.WriteLine($"  Đã tạo: {club} (Id: {club.Id})");

// ── Bước 2: Thêm 3 thành viên (US-01) ───────────────────────────
Title("Bước 2: Thêm 3 thành viên");
var an   = Member.Create("Nguyễn Văn An", "an@dthu.edu.vn", "DH24001");
var binh = Member.Create("Trần Thị Bình", "binh@dthu.edu.vn", "DH24002");
var chi  = Member.Create("Lê Minh Chi", "chi@dthu.edu.vn", "DH24003");
foreach (var m in new[] { an, binh, chi })
    Try($"{m} tham gia {club}", () => club.AddMember(m));
Console.WriteLine($"  Tổng số thành viên: {club.MemberCount()}");

// ── Bước 3: Tạo sự kiện (US-04) ─────────────────────────────────
// Cố ý để sức chứa = 2 (< 3 thành viên) để bước 4 thấy luật "hết chỗ".
Title("Bước 3: Tạo sự kiện");
var workshop = Event.Create(club.Id, "Workshop Git & GitHub", DateTime.Now.AddDays(7), capacity: 2);
Console.WriteLine($"  Đã tạo: {workshop} | sức chứa {workshop.Capacity} | " +
                  $"{(workshop.IsOnline ? "Online" : "Trực tiếp")}");

// ── Bước 4: Đăng ký sự kiện → Pending (US-02) ───────────────────
Title("Bước 4: Đăng ký (Pending)");
foreach (var m in new[] { an, binh, chi })
{
    Try($"{m} đăng ký \"{workshop}\"", () =>
        registrations.Add(Registration.Create(m, workshop, ActiveRegistrations(workshop))));
}
Console.WriteLine($"  Số phiếu hợp lệ: {ActiveRegistrations(workshop)}/{workshop.Capacity}");

// ── Bước 5: Xác nhận → Confirmed ────────────────────────────────
Title("Bước 5: Xác nhận (Confirmed)");
foreach (var r in registrations)
    Try($"Xác nhận phiếu {r.Id.ToString()[..8]}", r.Confirm);

// ── Bước 6: Check-in qua QR (US-03) ─────────────────────────────
// Chỉ An đến dự; Bình đã được xác nhận nhưng không đến.
Title("Bước 6: Check-in qua QR");
var regAn = registrations.First(r => r.MemberId == an.Id);
Try($"{an} check-in", () => checkIns.Add(CheckIn.Create(regAn, CheckInMethod.Qr)));

// ── Bước 7: Báo cáo tham dự (US-05) ─────────────────────────────
Title("Bước 7: Báo cáo tham dự");
var members = new[] { an, binh, chi };
Console.WriteLine($"  Sự kiện : {workshop}");
Console.WriteLine($"  Đăng ký : {ActiveRegistrations(workshop)}/{workshop.Capacity}");
Console.WriteLine($"  Check-in: {checkIns.Count}");
Console.WriteLine("  Chi tiết:");
foreach (var r in registrations.Where(r => r.EventId == workshop.Id))
{
    var who = members.First(m => m.Id == r.MemberId);
    var ci = checkIns.FirstOrDefault(c => c.RegistrationId == r.Id);
    var attendance = ci is null ? "chưa check-in" : $"đã check-in ({ci.Summary()})";
    Console.WriteLine($"   - {who,-28} {r.Status,-10} {attendance}");
}

// ── Bước 8: Thử vi phạm luật nghiệp vụ ──────────────────────────
// Mọi lệnh dưới đây đều PHẢI bị từ chối. Luật nằm trong entity, không phải trong Program.cs.
Title("Bước 8: Thử vi phạm luật nghiệp vụ");
var regBinh = registrations.First(r => r.MemberId == binh.Id);

Try("Thêm trùng thành viên vào CLB",              () => club.AddMember(an));
Try("Tạo Member với email sai",                   () => Member.Create("Phạm D", "abc", "DH24004"));
Try("Tạo Event với tiêu đề trống",                () => Event.Create(club.Id, "  ", DateTime.Now, 10));
Try("Tạo Event với sức chứa <= 0",                () => Event.Create(club.Id, "Seminar AI", DateTime.Now, 0));

regBinh.Cancel();
Try("Xác nhận lại phiếu đã huỷ",                  regBinh.Confirm);
Try("Check-in bằng phiếu đã huỷ",                 () => CheckIn.Create(regBinh));

var newbie = Member.Create("Võ Thị E", "e@dthu.edu.vn", "DH24005");
var pending = Registration.Create(newbie, workshop, ActiveRegistrations(workshop)); // Bình đã huỷ → còn chỗ
registrations.Add(pending);
Try("Check-in bằng phiếu còn Pending",            () => CheckIn.Create(pending));

an.Deactivate();                                  // US-06
Console.WriteLine($"  An còn hoạt động? {an.IsActive()} | lịch sử đăng ký còn: " +
                  $"{registrations.Count(r => r.MemberId == an.Id)} phiếu");
Try("Thành viên đã ngừng đăng ký sự kiện mới",    () => Registration.Create(an, workshop, 0));

Console.WriteLine("\nHoàn tất demo.");

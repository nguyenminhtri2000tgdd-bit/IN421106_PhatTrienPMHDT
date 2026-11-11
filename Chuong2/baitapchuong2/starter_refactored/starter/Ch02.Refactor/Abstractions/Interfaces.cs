namespace EduHub.Ch02.Abstractions;

// [GT] Toàn bộ file này là các "hợp đồng" (interface): chỉ nói LÀM ĐƯỢC GÌ, không có code làm sẵn.
// [GT] Class cấp cao (service) chỉ biết interface -> đó là DIP. Đổi bản cài đặt hay thay bản giả khi test đều không phải sửa service.

// ---------- Ghi log & thông báo (tách khỏi ClubManager: SRP + DIP) ----------
public interface IAppLogger
{
    void Info(string message);
    void Error(string message);
}

public interface INotificationService
{
    void Send(string to, string message);
}

// [GT] Danh sách kênh hợp lệ. Dùng enum thay cho chuỗi để không gõ sai tên kênh.
public enum NotificationChannel { Email, Sms, Slack }

// [GT] Factory (pattern Creational): nơi gọi chỉ nói "tôi cần kênh nào", nhà máy tự tạo đúng class.
public interface INotificationServiceFactory
{
    INotificationService Create(NotificationChannel channel);
}

// ---------- Kho dữ liệu ----------
// [GT] Tách IClubReader (chỉ đọc) khỏi IClubRepository (đọc + ghi) để:
// [GT]  - ISP: ReportService/EventService chỉ cần đọc thì chỉ phụ thuộc IClubReader.
// [GT]  - LSP: sau này có kho "chỉ đọc" thì chỉ ký IClubReader, không phải ném NotSupportedException ở Add.
public interface IClubReader
{
    IReadOnlyList<Club> GetAll();
    Club? GetById(Guid id);
}

public interface IClubRepository : IClubReader
{
    void Add(Club club);
}

public interface IMemberRepository
{
    void Add(Member member);
    IReadOnlyList<Member> GetByClub(Guid clubId);
}

public interface IEventRepository
{
    void Add(Event ev);
    Event? GetById(Guid id);
    IReadOnlyList<Event> GetByClub(Guid clubId);
}

// ---------- Service ----------
public interface IClubService
{
    Club CreateClub(string name, string description, string presidentEmail);
    Member AddMember(Guid clubId, string memberName, string email);
}

// [GT] ISP: chia interface "béo" thành các interface nhỏ. Máy check-in chỉ cần ký IEventCheckIn.
public interface IEventPlanner { Event CreateEvent(Guid clubId, string title, DateTime date); }
public interface IEventRegistration { bool Register(Guid eventId, Guid memberId); }
public interface IEventCheckIn { void CheckIn(Guid eventId, Guid memberId); }

// [GT] IEventService ghép các phần nhỏ lại + thêm Subscribe cho Observer.
public interface IEventService : IEventPlanner, IEventRegistration, IEventCheckIn
{
    void Subscribe(IEventObserver observer);
}

// ---------- Pattern ----------
// [GT] Observer: EventService (người phát tin) gọi OnEventCreated cho mọi observer đã đăng ký, không cần biết họ làm gì.
public interface IEventObserver
{
    void OnEventCreated(Event ev, Club club);
}

// [GT] Strategy: mỗi cách xuất báo cáo là một class riêng cài interface này.
public interface IReportStrategy
{
    string Name { get; }
    string Render(IReadOnlyList<ClubStat> stats);
}

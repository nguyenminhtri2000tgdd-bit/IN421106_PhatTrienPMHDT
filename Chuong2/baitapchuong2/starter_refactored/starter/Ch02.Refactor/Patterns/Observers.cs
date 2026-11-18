namespace EduHub.Ch02.Patterns;

// [GT] Các OBSERVER: mỗi class làm đúng MỘT việc khi có sự kiện mới. EventService không biết họ làm gì.

// [GT] Thay cho dòng File.AppendAllText(... "Event created") trong ClubManager.CreateEvent cũ.
public sealed class LogNotifier : IEventObserver
{
    private readonly IAppLogger _logger;
    public LogNotifier(IAppLogger logger) => _logger = logger;

    public void OnEventCreated(Event ev, Club club) =>
        _logger.Info($"Event created: {ev.Title}");
}

// [GT] Thay cho vòng foreach gửi "Email to {m.Email}: new event..." của ClubManager.CreateEvent cũ.
public sealed class EmailNotifier : IEventObserver
{
    private readonly IMemberRepository _members;
    private readonly INotificationService _email;   // [GT] nhận INotificationService (interface), không tự new EmailNotificationService

    public EmailNotifier(IMemberRepository members, INotificationService email)
    {
        _members = members;
        _email = email;
    }

    public void OnEventCreated(Event ev, Club club)
    {
        foreach (var m in _members.GetByClub(club.Id))
            _email.Send(m.Email, $"new event '{ev.Title}' at {club.Name}");
    }
}

// [GT] Observer "bổ sung" để minh hoạ OCP: muốn gửi thêm SMS khi có sự kiện, chỉ cần viết class này
// [GT] và gọi eventService.Subscribe(...) ở Program.cs. EventService KHÔNG phải sửa (câu hỏi ở slide 37).
public sealed class SmsNotifier : IEventObserver
{
    private readonly IMemberRepository _members;
    private readonly INotificationService _sms;

    public SmsNotifier(IMemberRepository members, INotificationService sms)
    {
        _members = members;
        _sms = sms;
    }

    public void OnEventCreated(Event ev, Club club)
    {
        foreach (var m in _members.GetByClub(club.Id))
            _sms.Send(m.Email, $"new event '{ev.Title}'");
    }
}

namespace EduHub.Ch02.Services;

// [GT] EventService thay cho ClubManager.CreateEvent. Đây là "người phát tin" (Subject) của pattern OBSERVER.
public sealed class EventService : IEventService
{
    private readonly IEventRepository _events;
    private readonly IClubReader _clubs;           // [GT] chỉ cần ĐỌC CLB nên chỉ phụ thuộc IClubReader (ISP)
    private readonly IMemberRepository _members;
    private readonly List<IEventObserver> _observers = new();   // [GT] danh sách "người đăng ký nhận tin"

    public EventService(IEventRepository events, IClubReader clubs, IMemberRepository members)
    {
        _events = events;
        _clubs = clubs;
        _members = members;
    }

    // [GT] Đăng ký một observer. Thêm người nhận tin mới KHÔNG phải sửa EventService.
    public void Subscribe(IEventObserver observer) => _observers.Add(observer);

    public Event CreateEvent(Guid clubId, string title, DateTime date)
    {
        var club = _clubs.GetById(clubId)
            ?? throw new InvalidOperationException("Club not found");

        var ev = new Event { Id = Guid.NewGuid(), ClubId = clubId, Title = title, Date = date };

        // [GT] LƯU Ý: ClubManager cũ tạo Event nhưng KHÔNG hề lưu vào đâu cả (chỉ return) nên không đăng ký/check-in được.
        // [GT] Giờ lưu vào repository để Register/CheckIn và báo cáo hoạt động được.
        _events.Add(ev);

        // [GT] OBSERVER: thay cho đoạn "tự gửi email cho từng thành viên" + "tự ghi log" của code cũ (dòng 94-97).
        // [GT] Chỉ việc báo cho mọi observer; việc ghi log và gửi email giờ nằm ở LogNotifier và EmailNotifier.
        // [GT] ToList() = duyệt trên bản sao, an toàn nếu observer đăng ký/huỷ trong lúc đang được gọi.
        foreach (var observer in _observers.ToList())
            observer.OnEventCreated(ev, club);

        return ev;
    }

    // [GT] Trả về bool theo Domain: true = đăng ký mới, false = đã đăng ký trước đó.
    public bool Register(Guid eventId, Guid memberId)
    {
        var ev = GetEvent(eventId);

        // [GT] Chỉ thành viên CỦA CLB tổ chức mới được đăng ký sự kiện.
        if (!_members.GetByClub(ev.ClubId).Any(m => m.Id == memberId))
            throw new InvalidOperationException("Member does not belong to this club");

        return ev.Register(memberId);   // [GT] luật trùng lặp nằm ở Domain
    }

    // [GT] Luật "chưa đăng ký thì không check-in được" nằm trong Event.CheckIn; service chỉ tìm sự kiện rồi gọi.
    public void CheckIn(Guid eventId, Guid memberId) => GetEvent(eventId).CheckIn(memberId);

    private Event GetEvent(Guid eventId) =>
        _events.GetById(eventId) ?? throw new InvalidOperationException("Event not found");
}

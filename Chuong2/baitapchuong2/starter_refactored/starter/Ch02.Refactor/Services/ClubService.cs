namespace EduHub.Ch02.Services;

// [GT] ClubService thay cho phần "quản lý CLB + thành viên" của ClubManager.
// [GT] SRP: class này CHỈ điều phối nghiệp vụ. Kiểm tra dữ liệu -> Domain; lưu -> repository; log -> IAppLogger; báo tin -> factory.
// [GT] DIP: mọi phụ thuộc là interface và được đưa vào qua constructor (Dependency Injection), không có "new" bên trong.
public sealed class ClubService : IClubService
{
    private readonly IClubRepository _clubs;
    private readonly IMemberRepository _members;
    private readonly INotificationServiceFactory _notifications;
    private readonly IAppLogger _logger;

    public ClubService(IClubRepository clubs, IMemberRepository members,
                       INotificationServiceFactory notifications, IAppLogger logger)
    {
        _clubs = clubs;
        _members = members;
        _notifications = notifications;
        _logger = logger;
    }

    // [GT] Thay cho ClubManager.CreateClub: trước đây một method làm 4 việc, giờ mỗi việc giao đúng chỗ (xem từng dòng).
    public Club CreateClub(string name, string description, string presidentEmail)
    {
        var club = Club.Create(name, description);   // [GT] Domain tự kiểm tra tên (rỗng / quá 100 ký tự)
        _clubs.Add(club);                             // [GT] repository lưu (có thể đang được Decorator bọc để ghi log)
        _logger.Info($"Club created: {club.Name}");   // [GT] logger ghi, giữ nguyên nội dung log cũ
        _notifications.Create(NotificationChannel.Email)   // [GT] FACTORY: nói "cần kênh Email", không if/else theo đuôi email nữa
                      .Send(presidentEmail, $"Club '{club.Name}' created");
        return club;
    }

    // [GT] Thay cho ClubManager.AddMemberToClub.
    public Member AddMember(Guid clubId, string memberName, string email)
    {
        // [GT] Giữ đúng lỗi cũ: không tìm thấy CLB -> InvalidOperationException("Club not found").
        var club = _clubs.GetById(clubId)
            ?? throw new InvalidOperationException("Club not found");

        // [GT] Luật "tối đa 50" đã chuyển vào Club.HasRoom; ở đây chỉ hỏi Domain, không còn số 50 viết cứng.
        if (!club.HasRoom(_members.GetByClub(clubId).Count))
            throw new InvalidOperationException($"Club is full (max {Club.MaxMembers} members)");

        var member = new Member { Id = Guid.NewGuid(), Name = memberName, Email = email, ClubId = clubId };
        _members.Add(member);

        _logger.Info($"Member {memberName} joined {club.Name}");

        // [GT] Code cũ gửi cứng "Welcome email" và "Slack" bằng Console.WriteLine ngay trong method (vi phạm OCP).
        // [GT] Giờ lấy kênh từ factory; thêm kênh SMS chỉ cần thêm một dòng tương tự, không phải sửa logic khác.
        _notifications.Create(NotificationChannel.Email).Send(email, $"Welcome to {club.Name}!");
        _notifications.Create(NotificationChannel.Slack).Send(club.Name, $"{memberName} joined {club.Name}");
        return member;
    }
}

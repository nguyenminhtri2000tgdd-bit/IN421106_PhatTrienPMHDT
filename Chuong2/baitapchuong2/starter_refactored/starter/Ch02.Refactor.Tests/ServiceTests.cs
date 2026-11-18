using EduHub.Ch02.Domain;
using EduHub.Ch02.Infrastructure;
using EduHub.Ch02.Patterns;
using EduHub.Ch02.Services;
using EduHub.Ch02.Abstractions;

namespace Ch02.Refactor.Tests;

// [GT] Test cho lớp Service. Dùng repository trong bộ nhớ thật + logger/notification giả.
public class ServiceTests
{
    // [GT] Hàm dựng chung để các test không lặp lại phần Arrange.
    private static (ClubService svc, FakeLogger log, FakeNotificationFactory mail, InMemoryClubRepository clubs, InMemoryMemberRepository members) Build()
    {
        var log = new FakeLogger();
        var mail = new FakeNotificationFactory();
        var clubs = new InMemoryClubRepository();
        var members = new InMemoryMemberRepository();
        return (new ClubService(clubs, members, mail, log), log, mail, clubs, members);
    }

    [Fact]
    public void CreateClub_ValidInput_SavesLogsAndNotifiesPresidentByEmail()
    {
        // Arrange
        var (svc, log, mail, clubs, _) = Build();

        // Act
        svc.CreateClub("Tech Club", "mô tả", "president@gmail.com");

        // Assert: đủ 3 việc: lưu, ghi log, báo tin (kênh Email)
        Assert.Single(clubs.GetAll());
        Assert.Contains(log.Messages, m => m.Contains("Club created: Tech Club"));
        var sent = Assert.Single(mail.Sent);
        Assert.Equal(NotificationChannel.Email, sent.Channel);
        Assert.Equal("president@gmail.com", sent.To);
    }

    [Fact]
    public void CreateClub_InvalidName_DoesNotSaveOrNotify()
    {
        // Arrange
        var (svc, _, mail, clubs, _) = Build();

        // Act
        var act = () => svc.CreateClub("   ", "mô tả", "p@gmail.com");

        // Assert: lỗi xảy ra TRƯỚC khi lưu hay gửi gì
        Assert.Throws<ArgumentException>(act);
        Assert.Empty(clubs.GetAll());
        Assert.Empty(mail.Sent);
    }

    [Fact]
    public void AddMember_WhenClubIsFull_ThrowsInvalidOperationException()
    {
        // Arrange: lấp đầy 50 thành viên
        var (svc, _, _, _, _) = Build();
        var club = svc.CreateClub("Tech Club", "mô tả", "p@gmail.com");
        for (int i = 0; i < Club.MaxMembers; i++)
            svc.AddMember(club.Id, $"SV{i}", $"sv{i}@dthu.edu.vn");

        // Act: thành viên thứ 51
        var act = () => svc.AddMember(club.Id, "Dư", "du@dthu.edu.vn");

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void AddMember_UnknownClub_ThrowsInvalidOperationException()
    {
        var (svc, _, _, _, _) = Build();

        var act = () => svc.AddMember(Guid.NewGuid(), "An", "a@dthu.edu.vn");

        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void AddMember_ValidInput_SendsWelcomeEmailAndSlack()
    {
        // Arrange
        var (svc, _, mail, _, members) = Build();
        var club = svc.CreateClub("Tech Club", "mô tả", "p@gmail.com");
        mail.Sent.Clear();   // [GT] bỏ thông báo của bước tạo CLB, chỉ xem thông báo của AddMember

        // Act
        svc.AddMember(club.Id, "An", "an@dthu.edu.vn");

        // Assert
        Assert.Single(members.GetByClub(club.Id));
        Assert.Contains(mail.Sent, s => s.Channel == NotificationChannel.Email && s.To == "an@dthu.edu.vn");
        Assert.Contains(mail.Sent, s => s.Channel == NotificationChannel.Slack);
    }

    [Fact]
    public void CreateEvent_NotifiesEverySubscribedObserver()
    {
        // Arrange
        var clubs = new InMemoryClubRepository();
        var club = Club.Create("Tech Club", "mô tả");
        clubs.Add(club);
        var events = new EventService(new InMemoryEventRepository(), clubs, new InMemoryMemberRepository());
        var o1 = new FakeObserver();
        var o2 = new FakeObserver();
        events.Subscribe(o1);
        events.Subscribe(o2);

        // Act
        events.CreateEvent(club.Id, "Workshop SOLID", new DateTime(2026, 11, 20));

        // Assert: CẢ HAI observer đều được báo, EventService không biết họ là ai
        Assert.Equal(1, o1.CallCount);
        Assert.Equal(1, o2.CallCount);
    }

    [Fact]
    public void EventService_Register_MemberNotInClub_ThrowsInvalidOperationException()
    {
        // Arrange
        var clubs = new InMemoryClubRepository();
        var club = Club.Create("Tech Club", "mô tả");
        clubs.Add(club);
        var events = new EventService(new InMemoryEventRepository(), clubs, new InMemoryMemberRepository());
        var ev = events.CreateEvent(club.Id, "Workshop", DateTime.Today);

        // Act
        // [GT] Register trả về bool nên viết lambda dạng khối { ... } (kiểu Action); nếu viết "() => events.Register(...)"
        // [GT] thì lambda trả về bool và Assert.Throws không nhận (lỗi biên dịch CS1503).
        var act = () => { events.Register(ev.Id, Guid.NewGuid()); };   // [GT] người lạ không thuộc CLB

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void ReportService_Generate_UsesStrategyAndLogsEachClub()
    {
        // Arrange
        var log = new FakeLogger();
        var clubs = new InMemoryClubRepository();
        clubs.Add(Club.Create("Tech Club", "mô tả"));
        var report = new ReportService(clubs, new InMemoryMemberRepository(), new InMemoryEventRepository(), log);

        // Act
        var text = report.Generate(new ConsoleReportStrategy());

        // Assert
        Assert.Contains("Club: Tech Club | Members: 0", text);
        Assert.Contains(log.Messages, m => m == "Report generated for Tech Club");   // [GT] giữ hành vi log cũ
    }
}

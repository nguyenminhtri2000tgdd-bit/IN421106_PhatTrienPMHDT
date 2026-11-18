using EduHub.Ch02.Abstractions;
using EduHub.Ch02.Domain;
using EduHub.Ch02.Infrastructure;
using EduHub.Ch02.Patterns;
using EduHub.Ch02.Services;

namespace Ch02.Refactor.Tests;

// [GT] Mỗi pattern có ít nhất một test chứng minh nó hoạt động đúng như ý định.
public class PatternTests
{
    // ---------- Factory ----------
    [Theory]
    [InlineData(NotificationChannel.Email, typeof(EmailNotificationService))]
    [InlineData(NotificationChannel.Sms, typeof(SmsNotificationService))]
    [InlineData(NotificationChannel.Slack, typeof(SlackNotificationService))]
    public void Factory_Create_ReturnsServiceMatchingChannel(NotificationChannel channel, Type expected)
    {
        // Arrange
        var factory = new NotificationServiceFactory();

        // Act
        var service = factory.Create(channel);

        // Assert: đúng loại class ứng với kênh
        Assert.IsType(expected, service);
    }

    [Fact]
    public void Factory_Create_UnknownChannel_ThrowsArgumentOutOfRangeException()
    {
        var factory = new NotificationServiceFactory();

        var act = () => factory.Create((NotificationChannel)99);   // [GT] ép số 99 thành enum để mô phỏng giá trị ngoài danh sách

        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    // ---------- Decorator ----------
    [Fact]
    public void LoggingClubRepository_Add_LogsAndDelegatesToInner()
    {
        // Arrange
        var log = new FakeLogger();
        var inner = new InMemoryClubRepository();
        var repo = new LoggingClubRepository(inner, log);

        // Act
        repo.Add(Club.Create("Tech Club", "mô tả"));

        // Assert: vừa GIAO cho bản thật (inner có dữ liệu) vừa GHI LOG
        Assert.Single(inner.GetAll());
        Assert.Contains(log.Messages, m => m.Contains("Add club 'Tech Club'"));
    }

    // ---------- Strategy ----------
    [Fact]
    public void CsvReportStrategy_Render_ProducesHeaderAndQuotedRow()
    {
        // Arrange
        var stats = new List<ClubStat> { new("Tech Club", 2, 1, 2, 1) };

        // Act
        var csv = new CsvReportStrategy().Render(stats);

        // Assert
        Assert.Equal("Club,Members,Events,Registered,CheckedIn\n\"Tech Club\",2,1,2,1", csv);
    }

    [Fact]
    public void CsvReportStrategy_Render_EscapesQuoteInClubName()
    {
        var stats = new List<ClubStat> { new("A \"B\" Club", 0, 0, 0, 0) };

        var csv = new CsvReportStrategy().Render(stats);

        Assert.Contains("\"A \"\"B\"\" Club\"", csv);   // [GT] dấu " trong tên được nhân đôi theo quy tắc CSV
    }

    [Fact]
    public void ConsoleReportStrategy_Render_ContainsClubNameAndMemberCount()
    {
        var stats = new List<ClubStat> { new("Tech Club", 2, 1, 2, 1) };

        var text = new ConsoleReportStrategy().Render(stats);

        Assert.Contains("Tech Club", text);
        Assert.Contains("Members: 2", text);
    }

    // ---------- Observer ----------
    [Fact]
    public void LogNotifier_OnEventCreated_WritesEventTitleToLog()
    {
        // Arrange
        var log = new FakeLogger();
        var notifier = new LogNotifier(log);
        var club = Club.Create("Tech Club", "mô tả");
        var ev = new Event { Title = "Workshop SOLID" };

        // Act
        notifier.OnEventCreated(ev, club);

        // Assert
        Assert.Contains(log.Messages, m => m.Contains("Workshop SOLID"));
    }

    [Fact]
    public void EmailNotifier_OnEventCreated_SendsToEveryMemberOfClub()
    {
        // Arrange
        var members = new InMemoryMemberRepository();
        var club = Club.Create("Tech Club", "mô tả");
        members.Add(new Member { Id = Guid.NewGuid(), Name = "A", Email = "a@x.vn", ClubId = club.Id });
        members.Add(new Member { Id = Guid.NewGuid(), Name = "B", Email = "b@x.vn", ClubId = club.Id });
        members.Add(new Member { Id = Guid.NewGuid(), Name = "Khác CLB", Email = "z@x.vn", ClubId = Guid.NewGuid() });
        var factory = new FakeNotificationFactory();
        var notifier = new EmailNotifier(members, factory.Create(NotificationChannel.Email));

        // Act
        notifier.OnEventCreated(new Event { Title = "Workshop" }, club);

        // Assert: chỉ 2 thành viên của CLB nhận, người ở CLB khác thì không
        Assert.Equal(2, factory.Sent.Count);
        Assert.DoesNotContain(factory.Sent, s => s.To == "z@x.vn");
    }
}

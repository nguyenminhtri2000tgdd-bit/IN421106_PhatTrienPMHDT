namespace EduHub.Ch02.Infrastructure;

// [GT] Mỗi kênh thông báo là MỘT class riêng cài INotificationService (OCP):
// [GT] muốn thêm kênh mới chỉ viết thêm class, không sửa code cũ (khác với chuỗi if/else Gmail/Yahoo trước đây).
public sealed class EmailNotificationService : INotificationService
{
    public void Send(string to, string message) => Console.WriteLine($"  [EMAIL] to {to}: {message}");
}

public sealed class SmsNotificationService : INotificationService
{
    public void Send(string to, string message) => Console.WriteLine($"  [SMS] to {to}: {message}");
}

public sealed class SlackNotificationService : INotificationService
{
    public void Send(string to, string message) => Console.WriteLine($"  [SLACK] to {to}: {message}");
}

// [GT] FACTORY: gom quyết định "tạo class nào" vào MỘT chỗ.
// [GT] Thay cho chuỗi if/else theo đuôi email ở ClubManager.CreateClub (dòng 52-63 của file cũ).
public sealed class NotificationServiceFactory : INotificationServiceFactory
{
    // [GT] "switch expression": mỗi kênh trả về một object tương ứng.
    public INotificationService Create(NotificationChannel channel) => channel switch
    {
        NotificationChannel.Email => new EmailNotificationService(),
        NotificationChannel.Sms => new SmsNotificationService(),
        NotificationChannel.Slack => new SlackNotificationService(),
        // [GT] Giá trị không hợp lệ thì ném lỗi ngay để lỗi lộ ra sớm, không im lặng bỏ qua.
        _ => throw new ArgumentOutOfRangeException(nameof(channel))
    };
}

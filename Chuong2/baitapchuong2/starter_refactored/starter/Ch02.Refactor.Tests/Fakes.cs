using EduHub.Ch02.Abstractions;
using EduHub.Ch02.Domain;

namespace Ch02.Refactor.Tests;

// [GT] "Bản giả" (fake) của các interface: chỉ ghi lại việc đã được gọi vào danh sách trong bộ nhớ.
// [GT] Nhờ DIP (service chỉ biết interface) nên test KHÔNG cần File, Console hay email thật -> nhanh, độc lập, chạy ở đâu cũng được.

public sealed class FakeLogger : IAppLogger
{
    public List<string> Messages { get; } = new();
    public void Info(string message) => Messages.Add(message);
    public void Error(string message) => Messages.Add("ERROR: " + message);
}

// [GT] Factory giả: mọi kênh đều trả về một "máy ghi" chép lại (kênh, người nhận, nội dung) để test kiểm tra.
public sealed class FakeNotificationFactory : INotificationServiceFactory
{
    public List<(NotificationChannel Channel, string To, string Message)> Sent { get; } = new();

    public INotificationService Create(NotificationChannel channel) => new Recorder(channel, Sent);

    private sealed class Recorder : INotificationService
    {
        private readonly NotificationChannel _channel;
        private readonly List<(NotificationChannel, string, string)> _sent;
        public Recorder(NotificationChannel channel, List<(NotificationChannel, string, string)> sent)
        {
            _channel = channel;
            _sent = sent;
        }
        public void Send(string to, string message) => _sent.Add((_channel, to, message));
    }
}

// [GT] Observer giả: chỉ đếm số lần được gọi.
public sealed class FakeObserver : IEventObserver
{
    public int CallCount { get; private set; }
    public void OnEventCreated(Event ev, Club club) => CallCount++;
}

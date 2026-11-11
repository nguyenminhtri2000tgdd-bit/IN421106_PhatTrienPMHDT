namespace EduHub.Ch02.Infrastructure;

// [GT] ConsoleLogger: in ra màn hình. Class cụ thể nên được phép gọi Console (logic nghiệp vụ thì không).
public sealed class ConsoleLogger : IAppLogger
{
    public void Info(string message) => Console.WriteLine(message);
    public void Error(string message) => Console.WriteLine($"[ERROR] {message}");
}

// [GT] FileLogger: giữ ĐÚNG hành vi cũ của ClubManager (ghi vào club-log.txt, dạng "[thời gian] nội dung").
// [GT] Trước đây File.AppendAllText nằm lẫn trong logic (vi phạm DIP); giờ chỉ nằm ở đúng một chỗ này.
public sealed class FileLogger : IAppLogger
{
    private readonly string _path;
    public FileLogger(string path) => _path = path;

    public void Info(string message) => File.AppendAllText(_path, $"[{DateTime.Now}] {message}\n");
    public void Error(string message) => File.AppendAllText(_path, $"[{DateTime.Now}] ERROR {message}\n");
}

// [GT] CompositeLogger: gộp nhiều logger (vừa in màn hình vừa ghi file) mà các service vẫn chỉ thấy MỘT IAppLogger.
// [GT] Cũng là minh hoạ DIP: thêm nơi ghi log mới không cần sửa ClubService/EventService.
public sealed class CompositeLogger : IAppLogger
{
    private readonly IAppLogger[] _loggers;
    public CompositeLogger(params IAppLogger[] loggers) => _loggers = loggers;

    public void Info(string message) { foreach (var l in _loggers) l.Info(message); }
    public void Error(string message) { foreach (var l in _loggers) l.Error(message); }
}

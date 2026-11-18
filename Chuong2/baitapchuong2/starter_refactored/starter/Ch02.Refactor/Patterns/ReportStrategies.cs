namespace EduHub.Ch02.Patterns;

// [GT] STRATEGY: mỗi định dạng báo cáo là một class riêng cùng cài IReportStrategy; ReportService đổi được lúc chạy.

// [GT] Cách 1: văn bản dễ đọc. Giữ dạng "Club: ... | Members: ..." của PrintClubReport cũ, thêm vài cột thống kê.
public sealed class ConsoleReportStrategy : IReportStrategy
{
    public string Name => "Console";

    public string Render(IReadOnlyList<ClubStat> stats) =>
        string.Join("\n", stats.Select(s =>
            $"Club: {s.ClubName} | Members: {s.Members} | Events: {s.Events} | Registered: {s.Registered} | CheckedIn: {s.CheckedIn}"));
}

// [GT] Cách 2: CSV (mở được bằng Excel).
public sealed class CsvReportStrategy : IReportStrategy
{
    public string Name => "CSV";

    public string Render(IReadOnlyList<ClubStat> stats) =>
        "Club,Members,Events,Registered,CheckedIn\n" + string.Join("\n",
            stats.Select(s => $"{Quote(s.ClubName)},{s.Members},{s.Events},{s.Registered},{s.CheckedIn}"));

    // [GT] Bọc tên trong dấu nháy kép; nếu tên có dấu nháy thì nhân đôi lên (quy tắc CSV) để không vỡ cột.
    private static string Quote(string text) => "\"" + text.Replace("\"", "\"\"") + "\"";
}

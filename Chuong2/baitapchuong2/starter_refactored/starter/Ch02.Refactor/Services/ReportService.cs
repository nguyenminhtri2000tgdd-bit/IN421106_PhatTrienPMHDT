namespace EduHub.Ch02.Services;

// [GT] Dữ liệu thống kê của một CLB. "record" = kiểu chỉ chứa dữ liệu, ngắn gọn và không đổi được.
public sealed record ClubStat(string ClubName, int Members, int Events, int Registered, int CheckedIn);

// [GT] ReportService thay cho ClubManager.PrintClubReport. Tính thống kê, còn VẼ ra định dạng nào là việc của Strategy.
public sealed class ReportService
{
    private readonly IClubReader _clubs;
    private readonly IMemberRepository _members;
    private readonly IEventRepository _events;
    private readonly IAppLogger _logger;

    public ReportService(IClubReader clubs, IMemberRepository members, IEventRepository events, IAppLogger logger)
    {
        _clubs = clubs;
        _members = members;
        _events = events;
        _logger = logger;
    }

    public IReadOnlyList<ClubStat> GetStats() =>
        _clubs.GetAll().Select(club =>
        {
            var events = _events.GetByClub(club.Id);
            return new ClubStat(
                club.Name,
                _members.GetByClub(club.Id).Count,
                events.Count,
                events.Sum(e => e.RegisteredMemberIds.Count),    // [GT] tổng lượt đăng ký của các sự kiện
                events.Sum(e => e.CheckedInMemberIds.Count));    // [GT] tổng lượt check-in
        }).ToList();

    // [GT] STRATEGY: nhận vào MỘT cách làm (Console, CSV...) rồi giao cho nó. Không có if/else theo định dạng.
    // [GT] Thêm PDF/JSON chỉ cần viết thêm strategy mới, ReportService không đổi (OCP).
    public string Generate(IReportStrategy strategy)
    {
        var stats = GetStats();

        // [GT] Giữ hành vi cũ "Report generated for {club}" nhưng qua IAppLogger chứ không gọi File trực tiếp.
        foreach (var s in stats)
            _logger.Info($"Report generated for {s.ClubName}");

        return strategy.Render(stats);
    }
}

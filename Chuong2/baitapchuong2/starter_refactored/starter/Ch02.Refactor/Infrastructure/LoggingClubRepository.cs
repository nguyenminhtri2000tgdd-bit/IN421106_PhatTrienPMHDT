namespace EduHub.Ch02.Infrastructure;

// [GT] DECORATOR (pattern Structural): "gói quà" thêm ghi log cho repository mà KHÔNG sửa InMemoryClubRepository.
// [GT] Điểm nhận dạng: class này vừa CÀI IClubRepository, vừa CHỨA một IClubRepository (_inner).
public sealed class LoggingClubRepository : IClubRepository
{
    private readonly IClubRepository _inner;   // [GT] repository thật bên trong
    private readonly IAppLogger _logger;

    public LoggingClubRepository(IClubRepository inner, IAppLogger logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public void Add(Club club)
    {
        _logger.Info($"[repo] Add club '{club.Name}'");   // [GT] việc thêm vào: ghi log trước
        _inner.Add(club);                                  // [GT] rồi GIAO LẠI cho bản thật làm việc chính
    }

    public IReadOnlyList<Club> GetAll()
    {
        _logger.Info("[repo] GetAll clubs");
        return _inner.GetAll();
    }

    public Club? GetById(Guid id)
    {
        _logger.Info($"[repo] GetById {id}");
        return _inner.GetById(id);
    }
}

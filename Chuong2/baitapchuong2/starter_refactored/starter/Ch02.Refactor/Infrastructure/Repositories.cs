namespace EduHub.Ch02.Infrastructure;

// [GT] Trước đây ClubManager tự giữ List<Club>, List<Member> bên trong (dòng 30-31) và còn trả nguyên _clubs ra ngoài.
// [GT] Giờ việc "lưu ở đâu" là của repository. Sau này đổi sang CSDL chỉ cần viết EfClubRepository mới, service không đổi.
// [GT] Bản InMemory = lưu trong bộ nhớ, đủ cho bài tập.

public sealed class InMemoryClubRepository : IClubRepository
{
    private readonly List<Club> _items = new();
    public void Add(Club club) => _items.Add(club);
    // [GT] Trả về BẢN SAO (ToList) để bên ngoài không sửa được danh sách nội bộ (khác GetAllClubs() => _clubs cũ).
    public IReadOnlyList<Club> GetAll() => _items.ToList();
    public Club? GetById(Guid id) => _items.FirstOrDefault(c => c.Id == id);
}

public sealed class InMemoryMemberRepository : IMemberRepository
{
    private readonly List<Member> _items = new();
    public void Add(Member member) => _items.Add(member);
    public IReadOnlyList<Member> GetByClub(Guid clubId) => _items.Where(m => m.ClubId == clubId).ToList();
}

public sealed class InMemoryEventRepository : IEventRepository
{
    private readonly List<Event> _items = new();
    public void Add(Event ev) => _items.Add(ev);
    public Event? GetById(Guid id) => _items.FirstOrDefault(e => e.Id == id);
    public IReadOnlyList<Event> GetByClub(Guid clubId) => _items.Where(e => e.ClubId == clubId).ToList();
}

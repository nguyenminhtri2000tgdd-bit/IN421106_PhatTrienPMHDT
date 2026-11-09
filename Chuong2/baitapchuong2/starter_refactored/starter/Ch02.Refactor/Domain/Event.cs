namespace EduHub.Ch02.Domain;

/// <summary>Sự kiện của CLB, kèm danh sách đăng ký và check-in.</summary>
public class Event
{
    public Guid Id { get; set; }
    public Guid ClubId { get; set; }
    public required string Title { get; set; }
    public DateTime Date { get; set; }

    public HashSet<Guid> RegisteredMemberIds { get; } = new();
    public HashSet<Guid> CheckedInMemberIds { get; } = new();

    // [GT] HashSet.Add trả về true nếu phần tử MỚI, false nếu đã có sẵn.
    // [GT] Nên "return Add(...)" đúng yêu cầu: đăng ký lần đầu -> true, đăng ký trùng -> false.
    public bool Register(Guid memberId) => RegisteredMemberIds.Add(memberId);

    public void CheckIn(Guid memberId)
    {
        // [GT] Luật nghiệp vụ: chỉ người ĐÃ đăng ký mới được check-in, ngược lại ném InvalidOperationException.
        if (!RegisteredMemberIds.Contains(memberId))
            throw new InvalidOperationException("Member has not registered for this event");

        // [GT] Dùng HashSet nên check-in hai lần cũng không bị trùng (lần sau Add trả false và bị bỏ qua).
        CheckedInMemberIds.Add(memberId);
    }
}

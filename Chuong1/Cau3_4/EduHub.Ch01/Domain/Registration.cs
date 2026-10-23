namespace EduHub.Ch01.Domain;

/// <summary>Phiếu đăng ký tham dự sự kiện.</summary>
public class Registration
{
    public Guid Id { get; private set; }
    public Guid MemberId { get; private set; }        // khoá ngoại → Member
    public Guid EventId { get; private set; }         // khoá ngoại → Event
    public DateTime RegisteredAt { get; private set; }
    public RegistrationStatus Status { get; private set; }

    private Registration(Guid memberId, Guid eventId)
    {
        Id = Guid.NewGuid();
        MemberId = memberId;
        EventId = eventId;
        RegisteredAt = DateTime.Now;
        Status = RegistrationStatus.Pending;          // luôn bắt đầu ở Pending
    }

    /// <summary>
    /// US-02: đăng ký sự kiện. Thành viên phải đang hoạt động và sự kiện còn chỗ.
    /// <paramref name="registeredCount"/> là số phiếu còn hiệu lực (không tính phiếu đã huỷ) của sự kiện.
    /// </summary>
    public static Registration Create(Member member, Event ev, int registeredCount)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(ev);

        if (!member.IsActive())
            throw new InvalidOperationException($"{member} đã ngừng hoạt động, không thể đăng ký");

        if (!ev.HasCapacity(registeredCount))
            throw new InvalidOperationException($"Sự kiện \"{ev}\" đã hết chỗ");

        return new Registration(member.Id, ev.Id);
    }

    /// <summary>Chỉ phiếu Pending mới được xác nhận.</summary>
    public void Confirm()
    {
        if (Status != RegistrationStatus.Pending)
            throw new InvalidOperationException($"Không thể xác nhận phiếu đang ở trạng thái {Status}");
        Status = RegistrationStatus.Confirmed;
    }

    /// <summary>Cho phép huỷ khi Pending hoặc Confirmed; Cancelled là trạng thái cuối.</summary>
    public void Cancel()
    {
        if (Status == RegistrationStatus.Cancelled)
            throw new InvalidOperationException("Phiếu đã được huỷ từ trước");
        Status = RegistrationStatus.Cancelled;
    }
}

namespace EduHub.Ch01.Domain;

/// <summary>Thành viên (sinh viên) của hệ thống EduHub.</summary>
public class Member
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string StudentCode { get; private set; }   // mã số sinh viên
    public DateTime JoinedAt { get; private set; }
    public MemberStatus Status { get; private set; }

    // Constructor private: không cho "new Member(...)" từ bên ngoài.
    private Member(string name, string email, string studentCode)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        StudentCode = studentCode;
        JoinedAt = DateTime.Now;
        Status = MemberStatus.Active;
    }

    /// <summary>Factory: kiểm tra dữ liệu rồi mới tạo Member.</summary>
    public static Member Create(string name, string email, string studentCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên trống", nameof(name));

        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            throw new ArgumentException("Email sai", nameof(email));

        if (string.IsNullOrWhiteSpace(studentCode))
            throw new ArgumentException("Mã số sinh viên trống", nameof(studentCode));

        return new Member(
            name.Trim(),
            email.Trim().ToLowerInvariant(),
            studentCode.Trim().ToUpperInvariant());
    }

    public bool IsActive() => Status == MemberStatus.Active;

    /// <summary>Ngừng hoạt động (US-06). Lịch sử đăng ký vẫn được giữ vì chỉ đổi trạng thái.</summary>
    public void Deactivate()
    {
        if (Status == MemberStatus.Inactive)
            throw new InvalidOperationException($"{Name} đã ngừng hoạt động từ trước");
        Status = MemberStatus.Inactive;
    }

    public override string ToString() => $"{Name} ({StudentCode})";

    // Kiểm tra email đơn giản: có đúng 1 dấu '@', có phần trước và sau, phần sau có dấu '.'.
    private static bool IsValidEmail(string email)
    {
        var e = email.Trim();
        var at = e.IndexOf('@');
        return at > 0
            && at == e.LastIndexOf('@')
            && at < e.Length - 1
            && e.IndexOf('.', at) > at + 1
            && !e.EndsWith('.');
    }
}

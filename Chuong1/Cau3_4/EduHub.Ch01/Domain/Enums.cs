namespace EduHub.Ch01.Domain;

/// <summary>Trạng thái của thành viên.</summary>
public enum MemberStatus
{
    Active,
    Inactive
}

/// <summary>Vòng đời của phiếu đăng ký: Pending → Confirmed → (CheckIn) hoặc → Cancelled.</summary>
public enum RegistrationStatus
{
    Pending,
    Confirmed,
    Cancelled
}

/// <summary>Các hình thức check-in hợp lệ (hằng số, tránh rải chuỗi khắp nơi).</summary>
public static class CheckInMethod
{
    public const string Manual = "Manual";
    public const string Qr = "QR";

    public static bool IsValid(string method) => method is Manual or Qr;
}

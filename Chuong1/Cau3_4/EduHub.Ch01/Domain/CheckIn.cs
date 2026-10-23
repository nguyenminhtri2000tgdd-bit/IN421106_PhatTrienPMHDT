namespace EduHub.Ch01.Domain;

/// <summary>Bản ghi điểm danh tại sự kiện (1 phiếu đăng ký có tối đa 1 CheckIn).</summary>
public class CheckIn
{
    public Guid Id { get; private set; }
    public Guid RegistrationId { get; private set; }  // khoá ngoại → Registration
    public DateTime CheckedInAt { get; private set; }
    public string Method { get; private set; }

    private CheckIn(Guid registrationId, string method)
    {
        Id = Guid.NewGuid();
        RegistrationId = registrationId;
        CheckedInAt = DateTime.Now;
        Method = method;
    }

    /// <summary>US-03: chỉ phiếu Confirmed mới được check-in.</summary>
    public static CheckIn Create(Registration registration, string method = CheckInMethod.Manual)
    {
        ArgumentNullException.ThrowIfNull(registration);

        if (registration.Status != RegistrationStatus.Confirmed)
            throw new InvalidOperationException(
                $"Chỉ phiếu Confirmed mới check-in (hiện là {registration.Status})");

        if (!CheckInMethod.IsValid(method))
            throw new ArgumentException($"Hình thức check-in không hợp lệ: {method}", nameof(method));

        return new CheckIn(registration.Id, method);
    }

    public string Summary() => $"{CheckedInAt:HH:mm:ss dd/MM/yyyy} · {Method}";
}

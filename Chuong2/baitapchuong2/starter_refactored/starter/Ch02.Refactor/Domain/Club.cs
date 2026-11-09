namespace EduHub.Ch02.Domain;

/// <summary>Câu lạc bộ. Quy tắc nghiệp vụ nên nằm ở đây, không nằm trong ClubManager.</summary>
public class Club
{
    public const int MaxMembers = 50;
    public const int MaxNameLength = 100;

    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }

    // [GT] Luật kiểm tra tên được CHUYỂN từ ClubManager.CreateClub (dòng 37-41 của file cũ) vào Domain.
    // [GT] Lợi ích: ai muốn tạo Club (service, test, console...) đều đi qua cổng này nên không thể tạo Club sai.
    public static Club Create(string name, string description)
    {
        // [GT] Tên rỗng / null / toàn khoảng trắng -> ArgumentException.
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Club name is required", nameof(name));

        // [GT] Dài hơn 100 ký tự thì bị từ chối. Dùng hằng MaxNameLength thay cho số 100 viết cứng (tránh "magic number").
        // [GT] Biên: đúng 100 ký tự vẫn hợp lệ, 101 ký tự mới bị từ chối.
        if (name.Length > MaxNameLength)
            throw new ArgumentException("Club name too long", nameof(name));

        return new Club { Id = Guid.NewGuid(), Name = name, Description = description };
    }

    // [GT] Luật "tối đa 50 thành viên" cũng được chuyển từ ClubManager.AddMemberToClub (dòng 72) vào Domain.
    // [GT] Dùng dấu "<" (không phải "<="): 49 thành viên còn chỗ, đủ 50 thì HẾT chỗ. Đây chính là giá trị biên cần test.
    public bool HasRoom(int currentMemberCount) => currentMemberCount < MaxMembers;
}

using EduHub.Ch02.Abstractions;
using EduHub.Ch02.Domain;
using EduHub.Ch02.Infrastructure;
using EduHub.Ch02.Services;

namespace Ch02.Refactor.Tests;

// Mẫu AAA: Arrange (chuẩn bị) - Act (thực hiện) - Assert (kiểm tra).
// Mỗi test chỉ kiểm tra MỘT hành vi. Đặt tên: TênMethod_TìnhHuống_KếtQuảMongĐợi.
public class DomainTests
{
    // [GT] Test mẫu của starter dùng "new ClubManager()". ClubManager đã được tách nhỏ nên test này
    // [GT] giữ nguyên ý nghĩa (tên rỗng -> ArgumentException) nhưng chạy trên ClubService với các bản giả.
    [Fact]
    public void CreateClub_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var service = new ClubService(new InMemoryClubRepository(), new InMemoryMemberRepository(),
                                      new FakeNotificationFactory(), new FakeLogger());

        // Act
        var act = () => service.CreateClub("", "mô tả", "p@gmail.com");

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    // ================= Club =================

    [Fact]
    public void Club_Create_WithValidName_ReturnsClub()
    {
        // Arrange
        const string name = "Tech Club";   // [GT] dữ liệu hợp lệ

        // Act
        var club = Club.Create(name, "Câu lạc bộ công nghệ");

        // Assert
        Assert.Equal(name, club.Name);                  // [GT] giữ đúng tên
        Assert.NotEqual(Guid.Empty, club.Id);           // [GT] Create phải sinh Id mới (khác Guid rỗng)
    }

    // [GT] [Theory] + [InlineData] = một test chạy nhiều bộ dữ liệu. Ở đây thử các tên "xấu": rỗng và toàn khoảng trắng.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Club_Create_WithInvalidName_ThrowsArgumentException(string name)
    {
        // Arrange: dữ liệu sai đã nằm trong tham số InlineData

        // Act
        var act = () => Club.Create(name, "mô tả");

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Club_Create_WithNameLongerThan100Chars_ThrowsArgumentException()
    {
        // Arrange: 101 ký tự = vượt giới hạn đúng 1 ký tự (giá trị biên)
        var tooLong = new string('a', Club.MaxNameLength + 1);

        // Act
        var act = () => Club.Create(tooLong, "mô tả");

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Club_Create_WithNameOfExactly100Chars_ReturnsClub()
    {
        // Arrange: đúng 100 ký tự = biên trên vẫn hợp lệ
        var maxLength = new string('a', Club.MaxNameLength);

        // Act
        var club = Club.Create(maxLength, "mô tả");

        // Assert
        Assert.Equal(Club.MaxNameLength, club.Name.Length);
    }

    // [GT] Test giá trị biên: 0 (rỗng), 49 (sát giới hạn), 50 (đúng giới hạn). Lỗi "<" thay vì "<=" chỉ lộ ra ở 49/50.
    [Theory]
    [InlineData(0, true)]
    [InlineData(49, true)]
    [InlineData(50, false)]
    public void Club_HasRoom_DependsOnCurrentMemberCount(int count, bool expected)
    {
        // Arrange
        var club = Club.Create("Tech Club", "mô tả");

        // Act
        var result = club.HasRoom(count);

        // Assert
        Assert.Equal(expected, result);
    }

    // [GT] Tên test theo mẫu starter: đúng giới hạn (50 thành viên) thì HẾT chỗ -> false.
    [Fact]
    public void Club_HasRoom_AtMaxMembers_ReturnsFalse()
    {
        // Arrange
        var club = Club.Create("Tech Club", "mô tả");

        // Act
        var hasRoom = club.HasRoom(Club.MaxMembers);

        // Assert
        Assert.False(hasRoom);
    }

    // ================= Event =================

    [Fact]
    public void Event_Register_NewMember_ReturnsTrue()
    {
        // Arrange
        var ev = new Event { Title = "Workshop SOLID" };
        var memberId = Guid.NewGuid();

        // Act
        var result = ev.Register(memberId);

        // Assert
        Assert.True(result);
        Assert.Contains(memberId, ev.RegisteredMemberIds);   // [GT] không chỉ kiểm tra true mà còn kiểm tra đã được ghi nhận
    }

    [Fact]
    public void Event_Register_SameMemberTwice_ReturnsFalse()
    {
        // Arrange
        var ev = new Event { Title = "Workshop SOLID" };
        var memberId = Guid.NewGuid();
        ev.Register(memberId);   // [GT] đăng ký lần 1

        // Act
        var secondTime = ev.Register(memberId);   // [GT] đăng ký lần 2 cùng người

        // Assert
        Assert.False(secondTime);
        Assert.Single(ev.RegisteredMemberIds);   // [GT] vẫn chỉ có 1 người, không bị nhân đôi
    }

    [Fact]
    public void Event_CheckIn_UnregisteredMember_ThrowsInvalidOperationException()
    {
        // Arrange
        var ev = new Event { Title = "Workshop SOLID" };

        // Act
        var act = () => ev.CheckIn(Guid.NewGuid());   // [GT] chưa đăng ký mà check-in

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    // [GT] Đường ĐÚNG của luật check-in (test tốt phủ cả đường đúng lẫn đường lỗi).
    [Fact]
    public void Event_CheckIn_RegisteredMember_RecordsCheckIn()
    {
        // Arrange
        var ev = new Event { Title = "Workshop SOLID" };
        var memberId = Guid.NewGuid();
        ev.Register(memberId);

        // Act
        ev.CheckIn(memberId);

        // Assert
        Assert.Contains(memberId, ev.CheckedInMemberIds);
    }
}

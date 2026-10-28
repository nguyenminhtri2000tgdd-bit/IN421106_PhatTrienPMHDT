using EduHub.Ch02.Domain;   // chỉnh theo namespace trong Club.cs của bạn
using Xunit;

namespace EduHub.Ch02.Tests.Domain;

public class ClubTests
{
    [Fact]
    public void Create_WithValidName_ReturnsClubWithNameAndNewId()
    {
        // Arrange
        const string name = "Tech Club";

        // Act
        var club = Club.Create(name, "Câu lạc bộ công nghệ");

        // Assert
        Assert.Equal(name, club.Name);
        Assert.NotEqual(Guid.Empty, club.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ThrowsArgumentException(string? name)
    {
        // Act
        var act = () => Club.Create(name!, "mô tả");

        // Assert
        Assert.ThrowsAny<ArgumentException>(act);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(49, true)]
    [InlineData(50, false)]   // biên: đủ 50 thì hết chỗ
    public void HasRoom_DependsOnCurrentMemberCount(int count, bool expected)
    {
        // Arrange
        var club = Club.Create("Tech Club", "mô tả");

        // Act
        var result = club.HasRoom(count);

        // Assert
        Assert.Equal(expected, result);
    }
}
using EduHub.Ch02.Domain;
using Xunit;

namespace EduHub.Ch02.Tests.Domain;

public class EventTests
{
    [Fact]
    public void CheckIn_UnregisteredMember_ThrowsInvalidOperationException()
    {
        // Arrange
        var ev = new Event { Title = "Workshop SOLID" };

        // Act
        var act = () => ev.CheckIn(Guid.NewGuid());   // chưa đăng ký mà check-in

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void CheckIn_RegisteredMember_Succeeds()
    {
        // Arrange
        var ev = new Event { Title = "Workshop SOLID" };
        var memberId = Guid.NewGuid();
        ev.Register(memberId);

        // Act
        ev.CheckIn(memberId);
    }
}
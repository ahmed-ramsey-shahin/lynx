using Lynx.IdentityService.Application.Common.Services;
using Moq;

namespace Lynx.IdentityService.Application.UnitTests.MockBuilders
{
    public class UserServiceMockBuilder
    {
        public Mock<IUserService> Mock { get; } = new(MockBehavior.Strict);
        public IUserService Object => Mock.Object;

        public UserServiceMockBuilder WithId(Guid? id=null)
        {
            Mock.Setup(service => service.UserId).Returns(id ?? Guid.NewGuid());
            return this;
        }
    }
}

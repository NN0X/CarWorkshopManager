using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CarWorkshopManager.Services.Implementations;
using SendGrid;
using SendGrid.Helpers.Mail;

public class EmailSenderTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IConfigurationSection> _mockSendGridSection;
    private readonly Mock<IConfigurationSection> _mockApiKeySection;
    private readonly Mock<IConfigurationSection> _mockSenderEmailSection;
    private readonly Mock<IConfigurationSection> _mockSenderNameSection;

    public EmailSenderTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockSendGridSection = new Mock<IConfigurationSection>();
        _mockApiKeySection = new Mock<IConfigurationSection>();
        _mockSenderEmailSection = new Mock<IConfigurationSection>();
        _mockSenderNameSection = new Mock<IConfigurationSection>();

        _mockConfiguration.Setup(c => c.GetSection("SendGrid")).Returns(_mockSendGridSection.Object);
        _mockSendGridSection.Setup(s => s.GetSection("ApiKey")).Returns(_mockApiKeySection.Object);
        _mockSendGridSection.Setup(s => s.GetSection("SenderEmail")).Returns(_mockSenderEmailSection.Object);
        _mockSendGridSection.Setup(s => s.GetSection("SenderName")).Returns(_mockSenderNameSection.Object);

        _mockApiKeySection.Setup(s => s.Value).Returns("SG.TestApiKey");
        _mockSenderEmailSection.Setup(s => s.Value).Returns("sender@example.com");
        _mockSenderNameSection.Setup(s => s.Value).Returns("Test Sender");
    }

    [Fact]
    public async Task SendEmailAsync_SendsEmailSuccessfully()
    {
        var email = "recipient@example.com";
        var subject = "Test Subject";
        var htmlMessage = "<h1>Hello World!</h1>";

        var emailSender = new EmailSender(_mockConfiguration.Object);

        await emailSender.SendEmailAsync(email, subject, htmlMessage);

        _mockApiKeySection.Verify(s => s.Value, Times.Once);
        _mockSenderEmailSection.Verify(s => s.Value, Times.Once);
        _mockSenderNameSection.Verify(s => s.Value, Times.Once);
    }

    [Fact]
    public void EmailSender_ThrowsException_IfApiKeyIsMissing()
    {
        _mockApiKeySection.Setup(s => s.Value).Returns((string)null!);

        var exception = Record.Exception(() => new EmailSender(_mockConfiguration.Object));
        Xunit.Assert.NotNull(exception);
        Xunit.Assert.IsType<InvalidOperationException>(exception);
        Xunit.Assert.Equal("SendGrid API Key is missing", exception.Message);
    }

    [Fact]
    public void EmailSender_ThrowsException_IfSenderEmailIsMissing()
    {
        _mockSenderEmailSection.Setup(s => s.Value).Returns((string)null!);

        var exception = Record.Exception(() => new EmailSender(_mockConfiguration.Object));
        Xunit.Assert.NotNull(exception);
        Xunit.Assert.IsType<InvalidOperationException>(exception);
        Xunit.Assert.Equal("Sender Email is missing", exception.Message);
    }

    [Fact]
    public void EmailSender_ThrowsException_IfSenderNameIsMissing()
    {
        _mockSenderNameSection.Setup(s => s.Value).Returns((string)null!);

        var exception = Record.Exception(() => new EmailSender(_mockConfiguration.Object));
        Xunit.Assert.NotNull(exception);
        Xunit.Assert.IsType<InvalidOperationException>(exception);
        Xunit.Assert.Equal("Sender Name is missing", exception.Message);
    }
}

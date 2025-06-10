using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CarWorkshopManager.Services.Implementations;

public class EmailSenderTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;

    public EmailSenderTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();

        _mockConfiguration.Setup(c => c["SendGrid:ApiKey"]).Returns("SG.TestApiKey");
        _mockConfiguration.Setup(c => c["SendGrid:SenderEmail"]).Returns("sender@example.com");
        _mockConfiguration.Setup(c => c["SendGrid:SenderName"]).Returns("Test Sender");
    }

    [Fact]
    public async Task SendEmailAsync_SendsEmailSuccessfully()
    {
        var email = "recipient@example.com";
        var subject = "Test Subject";
        var htmlMessage = "<h1>Hello World!</h1>";

        var emailSender = new EmailSender(_mockConfiguration.Object);
        await emailSender.SendEmailAsync(email, subject, htmlMessage);

        _mockConfiguration.Verify(c => c["SendGrid:ApiKey"], Times.Once);
        _mockConfiguration.Verify(c => c["SendGrid:SenderEmail"], Times.Once);
        _mockConfiguration.Verify(c => c["SendGrid:SenderName"], Times.Once);
    }

    [Fact]
    public void EmailSender_ThrowsException_IfApiKeyIsMissing()
    {
        _mockConfiguration.Setup(c => c["SendGrid:ApiKey"]).Returns((string?)null);

        var exception = Record.Exception(() => new EmailSender(_mockConfiguration.Object));

        Xunit.Assert.NotNull(exception);
        Xunit.Assert.IsType<InvalidOperationException>(exception);
        Xunit.Assert.Equal("SendGrid API Key is missing", exception.Message);
    }

    [Fact]
    public void EmailSender_ThrowsException_IfSenderEmailIsMissing()
    {
        _mockConfiguration.Setup(c => c["SendGrid:SenderEmail"]).Returns((string?)null);

        var exception = Record.Exception(() => new EmailSender(_mockConfiguration.Object));

        Xunit.Assert.NotNull(exception);
        Xunit.Assert.IsType<InvalidOperationException>(exception);
        Xunit.Assert.Equal("Sender Email is missing", exception.Message);
    }

    [Fact]
    public void EmailSender_ThrowsException_IfSenderNameIsMissing()
    {
        _mockConfiguration.Setup(c => c["SendGrid:SenderName"]).Returns((string?)null);

        var exception = Record.Exception(() => new EmailSender(_mockConfiguration.Object));

        Xunit.Assert.NotNull(exception);
        Xunit.Assert.IsType<InvalidOperationException>(exception);
        Xunit.Assert.Equal("Sender Name is missing", exception.Message);
    }
}

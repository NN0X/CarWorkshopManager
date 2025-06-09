namespace CarWorkshopManager.Services.Interfaces;

public interface IUsernameGeneratorService
{
    Task<string> GenerateUsernameAsync(string firstName, string lastName);
}
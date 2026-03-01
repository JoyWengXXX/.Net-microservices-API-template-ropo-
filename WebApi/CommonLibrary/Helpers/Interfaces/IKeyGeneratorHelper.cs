namespace CommonLibrary.Helpers.Interfaces
{
    public interface IKeyGeneratorHelper
    {
        string GenerateRandString(int length, bool isPassword, bool isSpecialSymbolsInclude);
        string GenerateRandomCapitalLetters();
        string GenerateSecurePassword(int length);
    }
}

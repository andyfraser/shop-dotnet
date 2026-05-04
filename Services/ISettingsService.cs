namespace ShopDotNet.Services;

public interface ISettingsService
{
    string Get(string key);
    Dictionary<string, string> GetAll();
    void Save(Dictionary<string, string> values);
}

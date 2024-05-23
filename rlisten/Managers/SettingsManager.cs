using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using rlisten.Services;

namespace rlisten.Managers;

public interface ISettingsManager
{
    T Read<T>() where T : new();
    void Write<T>(T settings) where T : class;
}

public class SettingsManager : ISettingsManager
{
    private readonly IConfiguration _configuration;
    private readonly IFileProvider _fileProvider;
    private readonly string _appSettingsPath;

    public SettingsManager(IConfiguration configuration, IFileProvider fileProvider)
    {
        _configuration = configuration;
        _fileProvider = fileProvider;
        _appSettingsPath = "appsettings.json";
    }

    public T Read<T>() where T : new()
    {
        var section = typeof(T).Name;
        T settings = new T();
        _configuration.GetSection(section).Bind(settings);
        return settings;
    }

    public void Write<T>(T settings) where T : class
    {
        var section = typeof(T).Name;
        try
        {
            var json = _fileProvider.ReadAllText(_appSettingsPath);
            var jsonObj = JObject.Parse(json);

            JObject jObject = JObject.FromObject(settings);
            jsonObj[section] = jObject;

            string output = jsonObj.ToString(Newtonsoft.Json.Formatting.Indented);
            _fileProvider.WriteAllText(_appSettingsPath, output);
            // _configuration.Reload(); // Consider reloading if using IConfigurationRoot
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Error writing settings.", ex);
        }
    }
}

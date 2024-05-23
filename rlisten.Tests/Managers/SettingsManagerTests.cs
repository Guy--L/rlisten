using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using rlisten.Managers;
using rlisten.Services;

namespace rlisten.Tests.Managers;

public class MySettings
{
    public string SettingValue { get; set; }
}

public class SettingsManagerTests
{
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<IFileProvider> _mockFileProvider;
    private readonly SettingsManager _settingsManager;
    private const string _appSettingsPath = "appsettings.json";

    public SettingsManagerTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockFileProvider = new Mock<IFileProvider>();
        _settingsManager = new SettingsManager(_mockConfig.Object, _mockFileProvider.Object);
    }

    [Fact]
    public void Read_Settings_ReturnsCorrectSettings()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string>
        {
            {"MySettings:SettingValue", "Test"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var settingsManager = new SettingsManager(configuration, _mockFileProvider.Object);

        // Act
        var result = settingsManager.Read<MySettings>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.SettingValue);
    }

    [Fact]
    public void Write_Settings_UpdatesAppSettingsFile()
    {
        // Arrange
        var settings = new MySettings { SettingValue = "Updated" };
        string json = "{ \"MySettings\": { \"SettingValue\": \"Original\" } }";
        var jsonObj = JObject.Parse(json);
        JObject jObject = JObject.FromObject(settings);

        // Set up file provider mocks
        _mockFileProvider.Setup(m => m.ReadAllText(_appSettingsPath)).Returns(json);
        _mockFileProvider.Setup(m => m.WriteAllText(_appSettingsPath, It.IsAny<string>()));

        // Act
        _settingsManager.Write(settings);

        // Assert
        _mockFileProvider.Verify(m => m.WriteAllText(_appSettingsPath, It.Is<string>(s => s.Contains("Updated"))), Times.Once());
    }
}

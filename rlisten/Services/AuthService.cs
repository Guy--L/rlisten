using Microsoft.Extensions.Configuration;
using Reddit;
using Reddit.AuthTokenRetriever;
using rlisten.Managers;
using rlisten.Models;
using System.Diagnostics;
using Reddit.AuthTokenRetriever.EventArgs;
using rlisten.Wrappers;
using System.Threading.Tasks;

namespace rlisten.Services;

public interface IAuthService
{
    Task<string> ReauthorizeAsync();
}

public class AuthService : IAuthService
{
    private readonly ISettingsManager settingsManager;
    private readonly IAuthTokenRetriever authTokenRetriever;
    private readonly IProcessWrapper processWrapper;
    private TaskCompletionSource<bool> authCompletionSource;
    private RedditOAuthTokens redditOAuthTokens;

    public AuthService(ISettingsManager settingsManager, IAuthTokenRetriever authTokenRetriever, IProcessWrapper processWrapper)
    {
        this.settingsManager = settingsManager;
        this.authTokenRetriever = authTokenRetriever;
        this.processWrapper = processWrapper;
        redditOAuthTokens = settingsManager.Read<RedditOAuthTokens>();
    }

    private void OnAuthSuccessful(object sender, AuthSuccessEventArgs e)
    {
        redditOAuthTokens.AccessToken = e.AccessToken;
        redditOAuthTokens.RefreshToken = e.RefreshToken;
        authCompletionSource?.TrySetResult(true);
    }

    public async Task<string> ReauthorizeAsync()
    {
        authCompletionSource = new TaskCompletionSource<bool>();
        authTokenRetriever.AuthSuccess += OnAuthSuccessful;

        authTokenRetriever.AwaitCallback();
        OpenBrowser(authTokenRetriever.AuthURL, authTokenRetriever.BrowserPath);

        await authCompletionSource.Task;
        authTokenRetriever.StopListening();
        authTokenRetriever.AuthSuccess -= OnAuthSuccessful;

        if (redditOAuthTokens.NeedsWrite())
            settingsManager.Write(redditOAuthTokens);

        return redditOAuthTokens.AccessToken;
    }

    private void OpenBrowser(string authUrl, string browserPath)
    {
        try
        {
            processWrapper.Start(new ProcessStartInfo(authUrl) { UseShellExecute = true });
        }
        catch (System.ComponentModel.Win32Exception)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(browserPath)
            {
                Arguments = authUrl
            };
            processWrapper.Start(processStartInfo);
        }
    }
}

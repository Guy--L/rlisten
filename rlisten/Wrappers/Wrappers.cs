using System;
using System.Diagnostics;
using Reddit;
using Reddit.AuthTokenRetriever;
using Reddit.AuthTokenRetriever.EventArgs;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;

namespace rlisten.Wrappers;

public interface IAuthTokenRetriever
{
    event EventHandler<AuthSuccessEventArgs> AuthSuccess;
    void AwaitCallback();
    string AuthURL { get; }
    string BrowserPath { get; }
    void StopListening();
}

public interface IProcessWrapper
{
    void Start(ProcessStartInfo processStartInfo);
}

public class ProcessWrapper : IProcessWrapper
{
    public void Start(ProcessStartInfo processStartInfo)
    {
        Process.Start(processStartInfo);
    }
}

public class AuthTokenRetrieverWrapper : IAuthTokenRetriever
{
    private readonly AuthTokenRetrieverLib _authTokenRetriever;
    private readonly string _browserPath;

    public AuthTokenRetrieverWrapper(AuthTokenRetrieverLib authTokenRetriever, string browserPath)
    {
        _authTokenRetriever = authTokenRetriever;
        _browserPath = browserPath;
    }

    public event EventHandler<AuthSuccessEventArgs> AuthSuccess
    {
        add { _authTokenRetriever.AuthSuccess += value; }
        remove { _authTokenRetriever.AuthSuccess -= value; }
    }

    public void AwaitCallback() => _authTokenRetriever.AwaitCallback();
    public void StopListening() => _authTokenRetriever.StopListening();
    public string BrowserPath => _browserPath;
    public string AuthURL => _authTokenRetriever.AuthURL();
}

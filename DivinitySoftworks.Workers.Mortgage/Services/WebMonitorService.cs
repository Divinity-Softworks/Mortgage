using DivinitySoftworks.Workers.Mortgage.Contracts.External.ING;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Text.Json;

namespace DivinitySoftworks.Workers.Mortgage.Services;

/// <summary>
/// Defines a service for monitoring web responses and fetching key-values data from a given URL.
/// </summary>
public interface IWebMonitorService {
    /// <summary>
    /// Asynchronously fetches key-value data from the specified URL.
    /// </summary>
    /// <param name="url">The URL to navigate to and monitor.</param>
    /// <returns>A task that represents the asynchronous operation, containing the key-values collection if found.</returns>
    Task<InterestRateCollection?> FetchAsync(string url);
}

/// <summary>
/// Implements the <see cref="IWebMonitorService"/> interface using Selenium WebDriver to monitor network responses.
/// </summary>
public sealed class WebMonitorService : IWebMonitorService {
    /// <summary>
    /// A task completion source used to signal the completion of the fetch operation.
    /// </summary>
    readonly TaskCompletionSource<InterestRateCollection?> taskCompletionSource;

    /// <summary>
    /// A cancellation token source to manage task cancellation.
    /// </summary>
    readonly CancellationTokenSource cancellationTokenSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebMonitorService"/> class,
    /// setting up the task completion and cancellation token sources.
    /// </summary>
    public WebMonitorService() {
        taskCompletionSource = new();
        cancellationTokenSource = new();
    }

    /// <summary>
    /// Handles the NetworkResponseReceived event to process network responses and extract key-values data.
    /// </summary>
    /// <param name="sender">The sender of the event, expected to be a <see cref="NetworkManager"/>.</param>
    /// <param name="e">The event arguments containing response data.</param>
    /// <exception cref="TypeLoadException">Thrown when the event sender is not of a <see cref="NetworkManager"/> type.</exception>
    private async void Network_NetworkResponseReceived(object? sender, NetworkResponseReceivedEventArgs e) {
        if (!e.ResponseUrl?.Contains("v2") is null) return;
        if (!e.ResponseUrl!.Contains("v2")) return;
        if (string.IsNullOrWhiteSpace(e.ResponseBody)) return;

        NetworkManager networkManager = sender as NetworkManager
            ?? throw new TypeLoadException("The event sender is not of a 'NetworkManager' type.");
        networkManager.NetworkResponseReceived -= Network_NetworkResponseReceived;
        await networkManager.StopMonitoring();
        cancellationTokenSource.Cancel();

        InterestRateCollection? rootobject = JsonSerializer.Deserialize<InterestRateCollection>(e.ResponseBody);
        taskCompletionSource.SetResult(rootobject);
    }

    /// <inheritdoc/>
    public async Task<InterestRateCollection?> FetchAsync(string url) {
        ChromeOptions chromeOptions = new();
        chromeOptions.AddArguments(new List<string>() {
            "--no-sandbox",
            "--disable-dev-shm-usage",
            "--disable-gpu",
            "--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36",
            "--headless=new",
            "--disable-cache", // Disable cache
            "--incognito", // Open in incognito mode, which doesn't use the cache
            "--disk-cache-size=0", // Disable disk cache entirely
            "--media-cache-size=0" // Disable media cache
        });

        ChromeDriverService service = ChromeDriverService.CreateDefaultService();
        service.HideCommandPromptWindow = true;

        using ChromeDriver driver = new(service, chromeOptions);
        driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
        WebDriverWait wait = new(driver, TimeSpan.FromSeconds(30));

        NetworkManager network = driver.Manage().Network as NetworkManager
            ?? throw new TypeLoadException("The Chrome Driver Network not of a 'NetworkManager' type.");

        await Task.Run(() => driver.Navigate().GoToUrl(url));

        IJavaScriptExecutor javaScriptExecutor = driver;


        IWebElement webElement = driver
            .FindElement(By.CssSelector("ing-app-open-page"))
            .GetShadowRoot()
            .QuerySelector(driver, ".ing-app-open-page")
            .QuerySelector(driver, "[data-tag-name='experience-renderer']")
            .GetShadowRoot()
            .QuerySelector(driver, "[data-tag-name='ing-default-layout']")
            .QuerySelector(driver, "[data-tag-name='ing-feat-frontend-mortgage-interest-rates-ing-feat-mortgage-actual-interest-rates']")
            .GetShadowRoot()
            .QuerySelector(driver, "main")
            .QuerySelector(driver, "div.wrapper2")
            .QuerySelector(driver, "ing-feat-mortgage-actual-interest-rates-component")
            .GetShadowRoot()
            .QuerySelector(driver, "ing-feat-actual-interest-rates-form")
            .GetShadowRoot()
            .QuerySelector(driver, "ing-form")
            .QuerySelector(driver, "form")
            .QuerySelector(driver, "div:nth-of-type(1)")
            .QuerySelector(driver, "div:nth-of-type(2)")
            .QuerySelector(driver, "ing-select")
            .QuerySelector(driver, "select");

        network.NetworkResponseReceived += Network_NetworkResponseReceived;

        await network.StartMonitoring();


        var element = new SelectElement(webElement);

        new SelectElement(webElement)
            .SelectByValue("EnergyLabel_Not_Available");


        Task.WaitAny(Task.Delay(TimeSpan.FromSeconds(30), cancellationTokenSource.Token), taskCompletionSource.Task);

        driver.CloseDevToolsSession();
        driver.Quit();

        if (taskCompletionSource.Task.IsCompleted)
            return await taskCompletionSource.Task;

        return null;
    }
}
namespace OpenQA.Selenium;

/// <summary>
/// Provides extension methods for working with <see cref="ISearchContext"/> elements in Selenium.
/// </summary>
public static class ISearchContextExtensions {
    /// <summary>
    /// Finds the first descendant element of the current <see cref="ISearchContext"/> that matches the specified CSS selector.
    /// </summary>
    /// <param name="searchContext">The search context (element or driver) to query within.</param>
    /// <param name="webDriver">The Selenium <see cref="IWebDriver"/> instance used to execute JavaScript.</param>
    /// <param name="cssSelector">The CSS selector string to match the element.</param>
    /// <returns>
    /// The first <see cref="IWebElement"/> that matches the specified CSS selector within the given search context.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="searchContext"/> or <paramref name="webDriver"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="cssSelector"/> is <c>null</c>, empty, or consists only of white-space characters.
    /// </exception>
    /// <exception cref="NotFoundException">
    /// Thrown if no element matching the <paramref name="cssSelector"/> is found.
    /// </exception>
    public static IWebElement QuerySelector(this ISearchContext searchContext, IWebDriver webDriver, string cssSelector) {
        ArgumentNullException.ThrowIfNull(searchContext);
        ArgumentNullException.ThrowIfNull(webDriver);
        if (string.IsNullOrWhiteSpace(cssSelector))
            throw new ArgumentException("CSS selector is required.", nameof(cssSelector));

        IJavaScriptExecutor javaScriptExecutor = (IJavaScriptExecutor)webDriver;
        return javaScriptExecutor.ExecuteScript("return arguments[0].querySelector(arguments[1]);", searchContext, cssSelector) as IWebElement
            ?? throw new NotFoundException($"Element [{cssSelector}] not found.");
    }
}

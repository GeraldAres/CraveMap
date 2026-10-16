using System.Text.Json;
using Microsoft.JSInterop;

namespace MidtermSprint.Web.Services;

public sealed class SavedRestaurantService(IJSRuntime jsRuntime)
{
    private const string StorageKey = "cravemap.savedRestaurants";
    private readonly SemaphoreSlim _gate = new(1, 1);
    private List<string> _slugs = [];
    private bool _isInitialized;

    public event Action? Changed;

    public async Task<IReadOnlyList<string>> GetSavedSlugsAsync(
        CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return Array.AsReadOnly(_slugs.ToArray());
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<bool> IsSavedAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var normalizedSlug = NormalizeSlug(slug);
        await EnsureInitializedAsync(cancellationToken);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return _slugs.Contains(normalizedSlug, StringComparer.OrdinalIgnoreCase);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<bool> ToggleAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var normalizedSlug = NormalizeSlug(slug);
        await EnsureInitializedAsync(cancellationToken);
        await _gate.WaitAsync(cancellationToken);
        bool isSaved;
        try
        {
            var updatedSlugs = _slugs.ToList();
            var existingIndex = updatedSlugs.FindIndex(
                savedSlug => string.Equals(savedSlug, normalizedSlug, StringComparison.OrdinalIgnoreCase));
            if (existingIndex >= 0)
            {
                updatedSlugs.RemoveAt(existingIndex);
                isSaved = false;
            }
            else
            {
                updatedSlugs.Insert(0, normalizedSlug);
                isSaved = true;
            }

            var serializedSlugs = JsonSerializer.Serialize(updatedSlugs);
            await jsRuntime.InvokeVoidAsync(
                "cravemapSaved.set",
                cancellationToken,
                StorageKey,
                serializedSlugs);
            _slugs = updatedSlugs;
        }
        finally
        {
            _gate.Release();
        }

        Changed?.Invoke();
        return isSaved;
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_isInitialized)
            {
                return;
            }

            var storedValue = await jsRuntime.InvokeAsync<string?>(
                "cravemapSaved.get",
                cancellationToken,
                StorageKey);

            if (storedValue is not null)
            {
                try
                {
                    var storedSlugs = JsonSerializer.Deserialize<List<string?>>(storedValue)
                        ?? throw new InvalidOperationException("The saved restaurant list is not a JSON array.");
                    if (storedSlugs.Any(string.IsNullOrWhiteSpace))
                    {
                        throw new InvalidOperationException("The saved restaurant list contains an empty slug.");
                    }

                    _slugs = storedSlugs
                        .Select(slug => NormalizeSlug(slug ?? throw new InvalidOperationException(
                            "The saved restaurant list contains an empty slug.")))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }
                catch (JsonException exception)
                {
                    throw new InvalidOperationException(
                        "CraveMap couldn't read the saved restaurant list in this browser. Clear this site's saved data and reload.",
                        exception);
                }
            }

            _isInitialized = true;
        }
        finally
        {
            _gate.Release();
        }
    }

    private static string NormalizeSlug(string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        return slug.Trim().ToLowerInvariant();
    }
}

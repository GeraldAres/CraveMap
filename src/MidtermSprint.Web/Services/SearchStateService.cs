using MidtermSprint.Application.Search;

namespace MidtermSprint.Web.Services;

public sealed class SearchStateService
{
    public event Action? Changed;

    public string Query { get; private set; } = string.Empty;
    public ManualSearchFilters Filters { get; private set; } = new();
    public SupportedLocation? SelectedLocation { get; private set; }
    public bool IsFilterDrawerOpen { get; private set; }

    public void SetQuery(string query)
    {
        Query = query;
        Changed?.Invoke();
    }

    public void SetFilters(ManualSearchFilters filters)
    {
        Filters = filters;
        Changed?.Invoke();
    }

    public void SetSelectedLocation(SupportedLocation location)
    {
        SelectedLocation = location;
        Changed?.Invoke();
    }

    public void OpenFilterDrawer()
    {
        IsFilterDrawerOpen = true;
        Changed?.Invoke();
    }

    public void CloseFilterDrawer()
    {
        IsFilterDrawerOpen = false;
        Changed?.Invoke();
    }
}

using JsonDispatcher.Models;
using JsonDispatcher.Services;

namespace JsonDispatcher;

public partial class MainPage : ContentPage
{
    private const string AllGenres = "Усі жанри";

    private readonly List<Book> _all = new();   
    private string? _path;                      
    private bool _dirty;                        
    private bool _updatingUi;                   

    private static readonly FilePickerFileType JsonType = new(
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI,       new[] { ".json" } },
            { DevicePlatform.Android,     new[] { "application/json" } },
            { DevicePlatform.iOS,         new[] { "public.json" } },
            { DevicePlatform.MacCatalyst, new[] { "public.json" } }
        });

    public MainPage()
    {
        InitializeComponent();
        RefreshGenres();
        ApplyFilter();
    }


    private async void OnOpen(object? sender, EventArgs e)
    {
        if (_dirty && !await DisplayAlert("Незбережені зміни", "Відкинути зміни та відкрити інший файл?", "Так", "Ні"))
            return;

        var file = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Оберіть JSON-файл",
            FileTypes = JsonType
        });
        if (file == null) return;

        try
        {
            using var stream = await file.OpenReadAsync();
            var loaded = JsonService.Load(stream);          
            _all.Clear();
            _all.AddRange(loaded);
            _path = file.FullPath;
            _dirty = false;
            PathLabel.Text = _path;
            RefreshGenres();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка читання", ex.Message, "OK");
        }
    }

    private async void OnSave(object? sender, EventArgs e)
    {
        if (_path == null) { OnSaveAs(sender, e); return; }
        try
        {
            JsonService.Save(_path, _all);                 
            _dirty = false;
            await DisplayAlert("Збережено", _path, "OK");
        }
        catch (Exception ex) { await DisplayAlert("Помилка запису", ex.Message, "OK"); }
    }

    private async void OnSaveAs(object? sender, EventArgs e)
    {
        string? name = await DisplayPromptAsync("Зберегти як", "Ім'я файлу:", initialValue: "books_copy.json");
        if (string.IsNullOrWhiteSpace(name)) return;
        if (!name.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) name += ".json";

        string dir = _path != null ? Path.GetDirectoryName(_path)! : FileSystem.AppDataDirectory;
        string target = Path.Combine(dir, name);
        try
        {
            JsonService.Save(target, _all);
            await DisplayAlert("Збережено", target, "OK");
        }
        catch (Exception ex) { await DisplayAlert("Помилка запису", ex.Message, "OK"); }
    }


    private async void OnAdd(object? sender, EventArgs e)
    {
        var page = new BookPage(null, Genres());
        await Navigation.PushModalAsync(page);
        var result = await page.Result;
        if (result == null) return;

        result.Id = _all.Count == 0 ? 1 : _all.Max(b => b.Id) + 1;
        _all.Add(result);
        _dirty = true;
        RefreshGenres();
        ApplyFilter();
    }

    private async void OnEdit(object? sender, EventArgs e)
    {
        if (BooksView.SelectedItem is not Book selected)
        {
            await DisplayAlert("Редагування", "Оберіть рядок у таблиці.", "OK");
            return;
        }
        var page = new BookPage(selected.Clone(), Genres());
        await Navigation.PushModalAsync(page);
        var result = await page.Result;
        if (result == null) return;

        int index = _all.IndexOf(selected);
        _all[index] = result;
        _dirty = true;
        RefreshGenres();
        ApplyFilter();
    }

    private async void OnDelete(object? sender, EventArgs e)
    {
        if (BooksView.SelectedItem is not Book selected)
        {
            await DisplayAlert("Видалення", "Оберіть рядок у таблиці.", "OK");
            return;
        }
        if (!await DisplayAlert("Видалення", $"Видалити «{selected.Title}»?", "Так", "Ні")) return;

        _all.Remove(selected);
        _dirty = true;
        RefreshGenres();
        ApplyFilter();
    }

    private async void OnAbout(object? sender, EventArgs e) =>
        await Navigation.PushModalAsync(new AboutPage());


    private void OnFilterChanged(object? sender, EventArgs e)
    {
        if (!_updatingUi) ApplyFilter();
    }

    private void OnResetFilter(object? sender, EventArgs e)
    {
        _updatingUi = true;
        SearchText.Text = YearFrom.Text = YearTo.Text = "";
        OnlyAvailable.IsChecked = false;
        GenrePicker.SelectedIndex = 0;
        _updatingUi = false;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        IEnumerable<Book> query = _all;

        string? text = SearchText.Text?.Trim();
        if (!string.IsNullOrEmpty(text))
            query = query.Where(b => b.Author.Contains(text, StringComparison.OrdinalIgnoreCase)
                                  || b.Title.Contains(text, StringComparison.OrdinalIgnoreCase));

        if (GenrePicker.SelectedItem is string genre && genre != AllGenres)
            query = query.Where(b => b.Genre == genre);

        if (int.TryParse(YearFrom.Text, out int from)) query = query.Where(b => b.Year >= from);
        if (int.TryParse(YearTo.Text, out int to)) query = query.Where(b => b.Year <= to);

        if (OnlyAvailable.IsChecked) query = query.Where(b => b.Available);

        var result = query.OrderBy(b => b.Title).ToList();
        BooksView.ItemsSource = result;
        CountLabel.Text = $"Показано {result.Count} із {_all.Count}";
    }

    private List<string> Genres() =>
        _all.Select(b => b.Genre).Where(g => !string.IsNullOrWhiteSpace(g)).Distinct().OrderBy(g => g).ToList();

    private void RefreshGenres()
    {
        _updatingUi = true;
        string? current = GenrePicker.SelectedItem as string;
        var items = new List<string> { AllGenres };
        items.AddRange(Genres());
        GenrePicker.ItemsSource = items;
        GenrePicker.SelectedIndex = current != null && items.Contains(current) ? items.IndexOf(current) : 0;
        _updatingUi = false;
    }
}
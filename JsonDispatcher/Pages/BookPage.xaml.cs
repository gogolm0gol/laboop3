using System.Globalization;
using JsonDispatcher.Models;

namespace JsonDispatcher;

public partial class BookPage : ContentPage
{
    private readonly TaskCompletionSource<Book?> _tcs = new();
    private readonly Book _book;

    public Task<Book?> Result => _tcs.Task;

    public BookPage(Book? book, IEnumerable<string> genres)
    {
        InitializeComponent();
        _book = book ?? new Book { Year = DateTime.Now.Year };
        Title = book == null ? "Додати книгу" : "Редагувати книгу";

        TitleEntry.Text = _book.Title;
        AuthorEntry.Text = _book.Author;
        YearEntry.Text = _book.Year.ToString();
        GenreEntry.Text = _book.Genre;
        PriceEntry.Text = _book.Price.ToString(CultureInfo.InvariantCulture);
        AvailableSwitch.IsToggled = _book.Available;
    }

    private async void OnOk(object? sender, EventArgs e)
    {
        string priceText = (PriceEntry.Text ?? "").Replace(',', '.');

        if (string.IsNullOrWhiteSpace(TitleEntry.Text) || string.IsNullOrWhiteSpace(AuthorEntry.Text))
        { await DisplayAlert("Помилка", "Заповніть назву та автора.", "OK"); return; }

        if (!int.TryParse(YearEntry.Text, out int year) || year < 1000 || year > DateTime.Now.Year + 1)
        { await DisplayAlert("Помилка", "Некоректний рік.", "OK"); return; }

        if (!decimal.TryParse(priceText, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price) || price < 0)
        { await DisplayAlert("Помилка", "Некоректна ціна.", "OK"); return; }

        _book.Title = TitleEntry.Text.Trim();
        _book.Author = AuthorEntry.Text.Trim();
        _book.Year = year;
        _book.Genre = (GenreEntry.Text ?? "").Trim();
        _book.Price = price;
        _book.Available = AvailableSwitch.IsToggled;

        _tcs.TrySetResult(_book);
        await Navigation.PopModalAsync();
    }

    private async void OnCancel(object? sender, EventArgs e)
    {
        _tcs.TrySetResult(null);
        await Navigation.PopModalAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _tcs.TrySetResult(null);
    }
}
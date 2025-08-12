using System.Collections.ObjectModel;

namespace TokeroApp;

public partial class DcaCalculatorPage : ContentPage
{
    public ObservableCollection<DcaResultRow> Results { get; set; } = new();

    public DcaCalculatorPage()
    {
        InitializeComponent();
        resultsView.ItemsSource = Results;
    }

    private string GetCoinIdFromPicker(string pickerValue)
    {
        if (pickerValue.Contains("Bitcoin")) return "bitcoin";
        if (pickerValue.Contains("Ethereum")) return "ethereum";
        if (pickerValue.Contains("Solana")) return "solana";
        if (pickerValue.Contains("Ripple")) return "ripple";
        return pickerValue.ToLower();
    }

    private async void OnCalculateClicked(object sender, EventArgs e)
    {
        // Clear previous results
        Results.Clear();
        summary.IsVisible = false;

        // Input validation
        if (cryptoPicker.SelectedIndex == -1 || 
            string.IsNullOrWhiteSpace(amountEntry.Text) || 
            string.IsNullOrWhiteSpace(dayEntry.Text))
        {
            await DisplayAlert("Missing Info", "Please fill in all fields.", "OK");
            return;
        }

        string crypto = cryptoPicker.SelectedItem.ToString();
        string coinId = GetCoinIdFromPicker(crypto);
        DateTime startDate = startDatePicker.Date;
        int dayOfMonth = int.Parse(dayEntry.Text);
        decimal monthlyAmount = decimal.Parse(amountEntry.Text);

        DateTime today = DateTime.Today;
        DateTime current = startDate;

        decimal? currentPrice = await App.Database.GetLatestPriceAsync(coinId, today);

        decimal totalCoins = 0m;
        decimal totalInvested = 0m;

        // Loop through each month from start to today
        while (current <= today)
        {
            decimal? buyPrice = await App.Database.GetLatestPriceAsync(coinId, current);
            

            if(buyPrice.HasValue && currentPrice.HasValue)
            {
                decimal coinAmount = monthlyAmount / buyPrice.Value;
                decimal valueToday = coinAmount * currentPrice.Value;

                Results.Add(new DcaResultRow
                {
                    Date = current.ToString("MMM yyyy"),
                    Invested = monthlyAmount,
                    CoinAmount = coinAmount,
                    ValueToday = valueToday
                });

                totalInvested += monthlyAmount;
                totalCoins += coinAmount;
            }
            current = current.AddMonths(1);
            current = new DateTime(current.Year, current.Month, Math.Min(dayOfMonth, DateTime.DaysInMonth(current.Year, current.Month)));
        }

        if (currentPrice.HasValue)
        {
            decimal portofolioValue = totalCoins * currentPrice.Value;

            totalInvesteLabel.Text = $"Total invested: €{totalInvested:F2}";
            totalCoinOwnedLabel.Text = $"Total coins owned: {totalCoins:F6}";
            currentValueLabel.Text = $"Current value of {crypto}: €{portofolioValue:F2}";
            portofolioTotalLabel.Text = $"Portofolio value: €{portofolioValue:F2}";
        }

        resultTitle.IsVisible = true;
        resultsView.IsVisible = true;
        summary.IsVisible = true;
    }
    
}

public class DcaResultRow
{
    public string Date { get; set; }
    public decimal Invested { get; set; }
    public decimal CoinAmount { get; set; }
    public decimal ValueToday { get; set; }

    public string InvestedText => $"Invested: €{Invested:F2}";
    public string CoinAmountText => $"Coin: {CoinAmount:F6}";
    public string ValueTodayText => $"Value Today: €{ValueToday:F2}";
    public string ROIText => $"ROI: €{(ValueToday - Invested):F2}";
}
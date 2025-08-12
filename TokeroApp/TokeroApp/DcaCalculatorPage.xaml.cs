using System.Collections.ObjectModel;
using TokeroApp.Model;

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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var coinList = await App.Database.GetDistinctCoinsAsync();

        cryptoCollection.ItemsSource = coinList;
    }

    private async void OnCalculateClicked(object sender, EventArgs e)
    {
        // Clear previous results
        Results.Clear();
        summary.IsVisible = false;

        var allCoins = (List<CryptoSelection>)cryptoCollection.ItemsSource;
        var selectedCoins = allCoins.Where(c => c.IsSelected).ToList();

        // Input validation
        if (!selectedCoins.Any() || 
            dayPicker.SelectedIndex == -1)
        {
            await DisplayAlert("Missing Info", "Please fill in all fields.", "OK");
            return;
        }

        int dayOfMonth = (int)dayPicker.SelectedItem;

        DateTime today = DateTime.Today;

        decimal totalPortofolio = 0m;
        decimal totalInvested = 0m;

        var coinSummaries = new List<string>();

        // Loop through each coin and each month from start to today
        foreach(var coin in selectedCoins)
        {
            string coinId = coin.Id;

            DateTime startDate = coin.StartDate;

            decimal monthlyAmount = coin.InvestmentAmount;
            decimal investedInCoin = 0m;
            decimal totalCoins = 0m;
            decimal? latestPrice = await App.Database.GetLatestPriceAsync(coinId, today);

            DateTime current = startDate;

            while(current <= today)
            {
                decimal? buyPrice = await App.Database.GetLatestPriceAsync(coinId, current);
                if(latestPrice.HasValue && buyPrice.HasValue)
                {
                    decimal coinAmount = monthlyAmount / buyPrice.Value;
                    decimal valueToday = coinAmount * latestPrice.Value;

                    Results.Add(new DcaResultRow
                    {   
                        Name = coin.Name,
                        Date = current.ToString("MMM yyyy"),
                        Invested = monthlyAmount,
                        CoinAmount = coinAmount,
                        ValueToday = valueToday
                    });

                    investedInCoin += monthlyAmount;
                    totalCoins += coinAmount;
                }

                current = current.AddMonths(1);
                current = new DateTime(current.Year, current.Month,
                    Math.Min(dayOfMonth, DateTime.DaysInMonth(current.Year, current.Month)));
            }
            if (latestPrice.HasValue)
            {
                decimal portofolioValue = totalCoins * latestPrice.Value;
                totalInvested += investedInCoin;
                totalPortofolio += portofolioValue;
                coinSummaries.Add($"{coin.Name}: {totalCoins:F6} coins, €{portofolioValue:F2}");
            }
        }
        totalInvesteLabel.Text = $"Total invested (all coins): €{totalInvested:F2}";
        totalCoinOwnedLabel.Text = string.Join("\n", coinSummaries);
        currentValueLabel.Text = $"Portofolio current value: €{totalPortofolio:F2}";
        portofolioTotalLabel.Text = $"Portofolio ROI: €{(totalPortofolio - totalInvested):F2}";

        resultTitle.IsVisible = true;
        resultsView.IsVisible = true;
        summary.IsVisible = true;
    }
}

public class DcaResultRow
{
    public string Name { get; set; }
    public string Date { get; set; }
    public decimal Invested { get; set; }
    public decimal CoinAmount { get; set; }
    public decimal ValueToday { get; set; }

    public string InvestedText => $"Invested: €{Invested:F2}";
    public string CoinAmountText => $"Coin: {CoinAmount:F6}";
    public string ValueTodayText => $"Value Today: €{ValueToday:F2}";
    public string ROIText => $"ROI: €{(ValueToday - Invested):F2}";
}
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

    private void OnCalculateClicked(object sender, EventArgs e)
    {
        // Clear previous results
        Results.Clear();

        // Input validation
        if (cryptoPicker.SelectedIndex == -1 || string.IsNullOrWhiteSpace(amountEntry.Text) || string.IsNullOrWhiteSpace(dayEntry.Text))
        {
            DisplayAlert("Missing Info", "Please fill in all fields.", "OK");
            return;
        }

        string crypto = cryptoPicker.SelectedItem.ToString();
        DateTime startDate = startDatePicker.Date;
        int dayOfMonth = int.Parse(dayEntry.Text);
        decimal monthlyAmount = decimal.Parse(amountEntry.Text);

        DateTime today = DateTime.Today;
        DateTime current = startDate;

        // Loop through each month from start to today
        while (current <= today)
        {
            // Simulate a price (replace this with real data or service)
            decimal fakePrice = GetMockPrice(crypto, current);
            decimal coinAmount = monthlyAmount / fakePrice;
            decimal currentPrice = GetMockPrice(crypto, today);
            decimal valueToday = coinAmount * currentPrice;

            Results.Add(new DcaResultRow
            {
                Date = current.ToString("MMM yyyy"),
                Invested = monthlyAmount,
                CoinAmount = coinAmount,
                ValueToday = valueToday
            });

            // Go to next month
            current = current.AddMonths(1);
            current = new DateTime(current.Year, current.Month, Math.Min(dayOfMonth, DateTime.DaysInMonth(current.Year, current.Month)));
        }

        resultTitle.IsVisible = true;
        resultsView.IsVisible = true;
    }

    // Mocked historical price
    private decimal GetMockPrice(string crypto, DateTime date)
    {
        int days = (DateTime.Today - date).Days;
        return crypto switch
        {
            string s when s.Contains("Bitcoin") => 20000 + (days % 3000),
            string s when s.Contains("Ethereum") => 1200 + (days % 800),
            string s when s.Contains("Solana") => 20 + (days % 30),
            string s when s.Contains("Ripple") => 0.5m + ((days % 100) * 0.01m),
            _ => 1000
        };
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
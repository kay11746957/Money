namespace Money.Models;

public class DcaResultModel
{
    public decimal TotalInvested { get; set; }
    public decimal TotalValue { get; set; }
    public decimal TotalReturn { get; set; }
    public List<YearlyData> YearlyBreakdown { get; set; } = new();
}

public class YearlyData
{
    public int Year { get; set; }
    public decimal Value { get; set; }
}

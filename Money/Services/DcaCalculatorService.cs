using Money.Models;

namespace Money.Services;

public class DcaCalculatorService
{
    public DcaResultModel Calculate(DcaInputModel input)
    {
        var yearlyBreakdown = new List<YearlyData>();
        decimal totalValue = 0;
        var monthlyRate = (decimal)input.AnnualRate / 100 / 12;
        var numberOfMonths = input.Years * 12;

        for (int i = 1; i <= numberOfMonths; i++)
        {
            totalValue += input.MonthlyInvestment;
            totalValue *= (1 + monthlyRate);

            if (i % 12 == 0)
            {
                yearlyBreakdown.Add(new YearlyData
                {
                    Year = i / 12,
                    Value = Math.Round(totalValue, 0)
                });
            }
        }

        var totalInvested = input.MonthlyInvestment * numberOfMonths;
        var totalReturn = totalValue - totalInvested;

        return new DcaResultModel
        {
            TotalInvested = Math.Round(totalInvested, 0),
            TotalValue = Math.Round(totalValue, 0),
            TotalReturn = Math.Round(totalReturn, 0),
            YearlyBreakdown = yearlyBreakdown
        };
    }
}

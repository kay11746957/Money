using System.ComponentModel.DataAnnotations;

namespace Money.Models;

public class DcaInputModel
{
    [Required(ErrorMessage = "月投入金額為必填欄位")]
    [Range(1, double.MaxValue, ErrorMessage = "月投入金額必須大於 0 元")]
    public decimal MonthlyInvestment { get; set; }

    [Required(ErrorMessage = "預期年化報酬率為必填欄位")]
    [Range(1, 15, ErrorMessage = "預期年化報酬率必須介於 1-15% 之間")]
    public int AnnualRate { get; set; }

    [Required(ErrorMessage = "投資年數為必填欄位")]
    [Range(1, 50, ErrorMessage = "投資年數最多 50 年")]
    public int Years { get; set; }
}

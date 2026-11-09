namespace PayCalculator;

/// <summary>
/// Provides methods and rate specifications for calculating monthly wages
/// according to standard Danish payroll rules.
/// </summary>
public sealed class PayrollCalculator
{
    /// <summary>
    /// Base hourly wage rate in DKK (148.17 DKK/hour).
    /// </summary>
    public const decimal BaseHourlyRate = 148.17m;

    /// <summary>
    /// Evening supplement rate in DKK (32.15 DKK/hour).
    /// </summary>
    public const decimal EveningSupplementRate = 32.15m;

    /// <summary>
    /// Holiday supplement rate in DKK (63.00 DKK/hour).
    /// </summary>
    public const decimal HolidaySupplementRate = 63.00m;

    /// <summary>
    /// Labor Market Contribution (AM-bidrag) rate (8.0%).
    /// </summary>
    public const decimal AmBidragRate = 0.08m;

    /// <summary>
    /// Monthly tax-exempt allowance (Personfradrag) in DKK (5,000.00 DKK).
    /// </summary>
    public const decimal MonthlyTaxDeduction = 5000.00m;

    /// <summary>
    /// Standard income tax (A-skat) rate (38.0%).
    /// </summary>
    public const decimal IncomeTaxRate = 0.38m;

    /// <summary>
    /// Fritvalgskonto contribution rate (10.0%).
    /// </summary>
    public const decimal FritvalgskontoRate = 0.10m;

    /// <summary>
    /// Configurable constant that determines whether the Fritvalgskonto contribution is calculated
    /// on Total Gross Pay (<c>true</c>) or Net Pay (<c>false</c>) by default.
    /// Defaults to <c>false</c> (calculated on Net Pay as specified).
    /// </summary>
    public const bool DefaultCalculateFritvalgOnGross = false;

    /// <summary>
    /// Calculates the complete monthly payroll breakdown from the worked hours.
    /// </summary>
    /// <remarks>
    /// Calculation order of operations:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       <b>Gross Pay (Bruttoløn):</b>
    ///       <c>BaseEarnings = BaseHours * 148.17</c>,
    ///       <c>EveningSupplement = EveningHours * 32.15</c>,
    ///       <c>HolidaySupplement = HolidayHours * 63.00</c>,
    ///       <c>TotalGrossPay = BaseEarnings + EveningSupplement + HolidaySupplement</c>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <b>Labor Market Contribution (8% AM-bidrag):</b>
    ///       <c>AmBidrag = TotalGrossPay * 0.08</c>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <b>Taxable Base (A-indkomst):</b>
    ///       <c>IncomeAfterAmBidrag = TotalGrossPay - AmBidrag</c>,
    ///       <c>TaxableBase = max(0, IncomeAfterAmBidrag - 5000.00)</c>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <b>Income Tax (38% A-skat):</b>
    ///       <c>IncomeTax = TaxableBase * 0.38</c>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <b>Net Payout (Udbetalt løn):</b>
    ///       <c>NetPay = TotalGrossPay - AmBidrag - IncomeTax</c>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <b>Fritvalgskonto Allocation (10%):</b>
    ///       <c>Fritvalgskonto = (calculateFritvalgOnGross ? TotalGrossPay : NetPay) * 0.10</c>
    ///     </description>
    ///   </item>
    /// </list>
    /// All monetary amounts are rounded to 2 decimal places using <see cref="MidpointRounding.AwayFromZero"/>.
    /// </remarks>
    /// <param name="baseHours">Total base hours worked (must be non-negative).</param>
    /// <param name="eveningHours">Total evening supplement hours worked (must be non-negative).</param>
    /// <param name="holidayHours">Total holiday supplement hours worked (must be non-negative).</param>
    /// <param name="calculateFritvalgOnGross">
    /// Optional flag indicating whether to calculate the 10% Fritvalgskonto contribution on gross pay (<c>true</c>)
    /// instead of net pay (<c>false</c>). Defaults to <see cref="DefaultCalculateFritvalgOnGross"/>.
    /// </param>
    /// <returns>A <see cref="PayrollResult"/> containing all rounded earnings, deductions, and totals.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any hours parameter is negative.</exception>
    public PayrollResult Calculate(
        decimal baseHours,
        decimal eveningHours,
        decimal holidayHours,
        bool calculateFritvalgOnGross = DefaultCalculateFritvalgOnGross)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(baseHours);
        ArgumentOutOfRangeException.ThrowIfNegative(eveningHours);
        ArgumentOutOfRangeException.ThrowIfNegative(holidayHours);

        // 1. Gross Pay (Bruttoløn)
        decimal baseEarnings = RoundCurrency(baseHours * BaseHourlyRate);
        decimal eveningSupplement = RoundCurrency(eveningHours * EveningSupplementRate);
        decimal holidaySupplement = RoundCurrency(holidayHours * HolidaySupplementRate);
        decimal totalGrossPay = RoundCurrency(baseEarnings + eveningSupplement + holidaySupplement);

        // 2. Labor Market Contribution (8% AM-bidrag)
        decimal amBidrag = RoundCurrency(totalGrossPay * AmBidragRate);

        // 3. Taxable Base (A-indkomst)
        decimal incomeAfterAmBidrag = RoundCurrency(totalGrossPay - amBidrag);
        decimal appliedTaxDeduction = Math.Min(incomeAfterAmBidrag, MonthlyTaxDeduction);
        decimal taxableBase = RoundCurrency(Math.Max(0m, incomeAfterAmBidrag - MonthlyTaxDeduction));

        // 4. Income Tax (38% A-skat)
        decimal incomeTax = RoundCurrency(taxableBase * IncomeTaxRate);

        // 5. Net Payout (Udbetalt løn)
        decimal netPay = RoundCurrency(totalGrossPay - amBidrag - incomeTax);

        // 6. Fritvalgskonto Allocation (10%)
        decimal fritvalgBasis = calculateFritvalgOnGross ? totalGrossPay : netPay;
        decimal fritvalgskontoContribution = RoundCurrency(fritvalgBasis * FritvalgskontoRate);

        return new PayrollResult
        {
            BaseHours = baseHours,
            EveningHours = eveningHours,
            HolidayHours = holidayHours,
            BaseEarnings = baseEarnings,
            EveningSupplement = eveningSupplement,
            HolidaySupplement = holidaySupplement,
            TotalGrossPay = totalGrossPay,
            AmBidrag = amBidrag,
            IncomeAfterAmBidrag = incomeAfterAmBidrag,
            MonthlyTaxDeduction = MonthlyTaxDeduction,
            AppliedTaxDeduction = appliedTaxDeduction,
            TaxableBase = taxableBase,
            IncomeTax = incomeTax,
            NetPay = netPay,
            FritvalgskontoContribution = fritvalgskontoContribution,
            IsFritvalgBasedOnGross = calculateFritvalgOnGross
        };
    }

    /// <summary>
    /// Rounds a monetary value to 2 decimal places using <see cref="MidpointRounding.AwayFromZero"/>.
    /// </summary>
    /// <param name="amount">The raw monetary amount to round.</param>
    /// <returns>The value rounded to 2 decimal places.</returns>
    public static decimal RoundCurrency(decimal amount) =>
        Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}

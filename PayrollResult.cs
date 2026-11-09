namespace PayCalculator;

/// <summary>
/// Represents the complete breakdown of an employee's monthly wage calculation
/// according to Danish payroll rules.
/// </summary>
public sealed record PayrollResult
{
    /// <summary>
    /// Gets the total base hours worked.
    /// </summary>
    public required decimal BaseHours { get; init; }

    /// <summary>
    /// Gets the evening supplement hours worked.
    /// </summary>
    public required decimal EveningHours { get; init; }

    /// <summary>
    /// Gets the holiday supplement hours worked.
    /// </summary>
    public required decimal HolidayHours { get; init; }

    /// <summary>
    /// Gets the base earnings (<c>BaseHours * BaseHourlyRate</c>), rounded to 2 decimal places.
    /// </summary>
    public required decimal BaseEarnings { get; init; }

    /// <summary>
    /// Gets the evening supplement earnings (<c>EveningHours * EveningSupplementRate</c>), rounded to 2 decimal places.
    /// </summary>
    public required decimal EveningSupplement { get; init; }

    /// <summary>
    /// Gets the holiday supplement earnings (<c>HolidayHours * HolidaySupplementRate</c>), rounded to 2 decimal places.
    /// </summary>
    public required decimal HolidaySupplement { get; init; }

    /// <summary>
    /// Gets the total gross pay (Bruttoløn): <c>BaseEarnings + EveningSupplement + HolidaySupplement</c>.
    /// </summary>
    public required decimal TotalGrossPay { get; init; }

    /// <summary>
    /// Gets the 8% Labor Market Contribution (AM-bidrag): <c>TotalGrossPay * 0.08</c>.
    /// </summary>
    public required decimal AmBidrag { get; init; }

    /// <summary>
    /// Gets the income remaining after deducting AM-bidrag: <c>TotalGrossPay - AmBidrag</c>.
    /// </summary>
    public required decimal IncomeAfterAmBidrag { get; init; }

    /// <summary>
    /// Gets the configured monthly tax-exempt allowance (Personfradrag).
    /// </summary>
    public required decimal MonthlyTaxDeduction { get; init; }

    /// <summary>
    /// Gets the actual tax deduction applied (capped at <see cref="IncomeAfterAmBidrag"/>).
    /// </summary>
    public required decimal AppliedTaxDeduction { get; init; }

    /// <summary>
    /// Gets the taxable base (A-indkomst after personfradrag): <c>max(0, IncomeAfterAmBidrag - MonthlyTaxDeduction)</c>.
    /// </summary>
    public required decimal TaxableBase { get; init; }

    /// <summary>
    /// Gets the 38% standard income tax (A-skat): <c>TaxableBase * 0.38</c>.
    /// </summary>
    public required decimal IncomeTax { get; init; }

    /// <summary>
    /// Gets the net take-home pay (Udbetalt løn): <c>TotalGrossPay - AmBidrag - IncomeTax</c>.
    /// </summary>
    public required decimal NetPay { get; init; }

    /// <summary>
    /// Gets the 10% Fritvalgskonto contribution, calculated on either net pay or gross pay depending on configuration.
    /// </summary>
    public required decimal FritvalgskontoContribution { get; init; }

    /// <summary>
    /// Gets a value indicating whether the Fritvalgskonto contribution was calculated on gross pay (<c>true</c>) or net pay (<c>false</c>).
    /// </summary>
    public required bool IsFritvalgBasedOnGross { get; init; }
}

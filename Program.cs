using System.Globalization;
using System.Text;

namespace PayCalculator;

/// <summary>
/// Entry point and console input/output coordinator for the Danish Payroll Calculator.
/// </summary>
public static class Program
{
    private static readonly CultureInfo DisplayCulture = CultureInfo.InvariantCulture;

    /// <summary>
    /// Application entry point. Coordinates user input, wage calculation, and pay slip rendering.
    /// </summary>
    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        PrintHeader();

        decimal baseHours = PromptNonNegativeValue("1. Total base hours worked        : ");
        decimal eveningHours = PromptNonNegativeValue("2. Evening supplement hours       : ");
        decimal holidayHours = PromptNonNegativeValue("3. Holiday supplement hours       : ");
        decimal monthlyTaxDeduction = PromptNonNegativeValue("4. Monthly tax exemption (Fradrag): ");

        var calculator = new PayrollCalculator();
        PayrollResult result = calculator.Calculate(baseHours, eveningHours, holidayHours, monthlyTaxDeduction);

        PrintPaySlip(result);
    }

    /// <summary>
    /// Repeatedly prompts the user until a valid, non-negative decimal value is entered.
    /// Supports both comma (<c>,</c>) and period (<c>.</c>) as decimal separators.
    /// </summary>
    /// <param name="prompt">The label displayed to the user.</param>
    /// <returns>A validated non-negative <see cref="decimal"/> value.</returns>
    public static decimal PromptNonNegativeValue(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? rawInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(rawInput))
            {
                WriteError("   [Error] Input cannot be empty. Please enter a numeric value (e.g., 160.33 or 37,5).");
                continue;
            }

            if (!TryParseDecimal(rawInput, out decimal value))
            {
                WriteError("   [Error] Invalid number format. Please enter a valid number using '.' or ',' as the decimal separator.");
                continue;
            }

            if (value < 0m)
            {
                WriteError("   [Error] Value cannot be negative. Please enter 0 or a positive value.");
                continue;
            }

            return value;
        }
    }

    /// <summary>
    /// Attempts to parse a user-supplied string into a decimal value, allowing either
    /// comma (<c>,</c>) or period (<c>.</c>) as the decimal separator via
    /// <see cref="CultureInfo.InvariantCulture"/> and <c>da-DK</c> rules.
    /// </summary>
    /// <param name="input">The raw user input string.</param>
    /// <param name="value">When this method returns, contains the parsed decimal value if successful.</param>
    /// <returns><c>true</c> if <paramref name="input"/> was successfully parsed; otherwise, <c>false</c>.</returns>
    public static bool TryParseDecimal(string? input, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        string trimmed = input.Trim();

        // Reject inputs that mix both separators or contain multiple separators to prevent ambiguity
        int dotCount = trimmed.Count(c => c == '.');
        int commaCount = trimmed.Count(c => c == ',');
        if (dotCount + commaCount > 1)
        {
            return false;
        }

        const NumberStyles allowedStyles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

        // Parse using da-DK if comma is present, or InvariantCulture if period (or integer) is present
        CultureInfo parseCulture = commaCount == 1
            ? CultureInfo.GetCultureInfo("da-DK")
            : CultureInfo.InvariantCulture;

        return decimal.TryParse(trimmed, allowedStyles, parseCulture, out value);
    }

    /// <summary>
    /// Prints the application banner and rate overview to the console.
    /// </summary>
    private static void PrintHeader()
    {
        Console.WriteLine("======================================================================");
        Console.WriteLine("                 DANISH MONTHLY PAYROLL CALCULATOR                    ");
        Console.WriteLine("======================================================================");
        Console.WriteLine($"  Base Hourly Rate      : {FormatCurrency(PayrollCalculator.BaseHourlyRate)}/hour");
        Console.WriteLine($"  Evening Supplement    : {FormatCurrency(PayrollCalculator.EveningSupplementRate)}/hour");
        Console.WriteLine($"  Holiday Supplement    : {FormatCurrency(PayrollCalculator.HolidaySupplementRate)}/hour");
        Console.WriteLine($"  AM-bidrag             : {PayrollCalculator.AmBidragRate:P1}");
        Console.WriteLine($"  A-skat                : {PayrollCalculator.IncomeTaxRate:P1}");
        Console.WriteLine($"  Fritvalgskonto        : {PayrollCalculator.FritvalgskontoRate:P1}");
        Console.WriteLine("----------------------------------------------------------------------");
        Console.WriteLine("Enter worked hours and tax exemption below (both '.' and ',' are accepted):");
        Console.WriteLine();
    }

    /// <summary>
    /// Renders the formatted monthly pay slip to the console.
    /// </summary>
    /// <param name="result">The calculated <see cref="PayrollResult"/> to display.</param>
    public static void PrintPaySlip(PayrollResult result)
    {
        string fritvalgBasisLabel = result.IsFritvalgBasedOnGross ? "of Gross Pay" : "of Net Pay";

        Console.WriteLine();
        Console.WriteLine("======================================================================");
        Console.WriteLine("                          MONTHLY PAY SLIP                            ");
        Console.WriteLine("======================================================================");
        Console.WriteLine("EARNINGS BREAKDOWN");
        Console.WriteLine("----------------------------------------------------------------------");
        PrintLine(
            $"Base Pay ({FormatHours(result.BaseHours)} hrs × {PayrollCalculator.BaseHourlyRate.ToString("N2", DisplayCulture)} DKK)",
            FormatCurrency(result.BaseEarnings));
        PrintLine(
            $"Evening Supplement ({FormatHours(result.EveningHours)} hrs × {PayrollCalculator.EveningSupplementRate.ToString("N2", DisplayCulture)} DKK)",
            FormatCurrency(result.EveningSupplement));
        PrintLine(
            $"Holiday Supplement ({FormatHours(result.HolidayHours)} hrs × {PayrollCalculator.HolidaySupplementRate.ToString("N2", DisplayCulture)} DKK)",
            FormatCurrency(result.HolidaySupplement));
        Console.WriteLine("----------------------------------------------------------------------");
        PrintLine("Total Gross Pay (Bruttoløn)", FormatCurrency(result.TotalGrossPay));

        Console.WriteLine();
        Console.WriteLine("DEDUCTIONS");
        Console.WriteLine("----------------------------------------------------------------------");
        PrintLine("AM-Bidrag (8%)", $"-{FormatCurrency(result.AmBidrag)}");
        PrintLine("Income after AM-Bidrag (A-indkomst)", FormatCurrency(result.IncomeAfterAmBidrag));
        PrintLine(
            $"Applied Monthly Deduction (Fradrag: {FormatCurrency(result.MonthlyTaxDeduction)})",
            $"-{FormatCurrency(result.AppliedTaxDeduction)}");
        PrintLine("Taxable Amount (Skattepligtig indkomst)", FormatCurrency(result.TaxableBase));
        PrintLine("A-Skat (38%)", $"-{FormatCurrency(result.IncomeTax)}");

        Console.WriteLine();
        Console.WriteLine("TOTALS");
        Console.WriteLine("----------------------------------------------------------------------");
        PrintLine("Net Take-Home Pay (Udbetaling)", FormatCurrency(result.NetPay));
        PrintLine(
            $"Fritvalgskonto Contribution (10% {fritvalgBasisLabel})",
            FormatCurrency(result.FritvalgskontoContribution));
        Console.WriteLine("======================================================================");
    }

    private static void PrintLine(string label, string amount)
    {
        Console.WriteLine($"  {label,-48} {amount,18}");
    }

    private static string FormatCurrency(decimal value) =>
        $"{value.ToString("N2", DisplayCulture)} DKK";

    private static string FormatHours(decimal hours) =>
        hours.ToString("0.##", DisplayCulture);

    private static void WriteError(string message)
    {
        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ForegroundColor = originalColor;
    }
}

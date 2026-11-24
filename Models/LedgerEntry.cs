namespace WebApplication1.Models;

/// <summary>รายรับหรือรายจ่ายที่ฝ่ายบัญชีบันทึกเองในตาราง ledger_entries</summary>
public class LedgerEntry
{
    public const string Income = "income";
    public const string Expense = "expense";

    public int Id { get; set; }

    public DateOnly EntryDate { get; set; }

    public string EntryType { get; set; } = Expense;

    public string Title { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string? Note { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

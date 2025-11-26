using ShamDevs.EFatoraJo.Models;

namespace EFatoraJoConsoleApp.Models;

/// <summary>
/// Represents parsed return invoice input with the required metadata and original invoice payload.
/// </summary>
public class ReturnInvoiceInput
{
    public string OriginalInvoiceNumber { get; init; } = string.Empty;
    public string ReturnInvoiceNumber { get; init; } = string.Empty;
    public string ReturnReason { get; init; } = string.Empty;
    public string? ReturnUUID { get; init; }
    public string ReturnInvoiceDate { get; init; } = string.Empty;
    public Invoice OriginalInvoice { get; init; } = null!;
}

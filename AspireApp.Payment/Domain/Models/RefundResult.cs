namespace AspireApp.Payment.Domain.Models;

/// <summary>
/// Result of a refund operation
/// </summary>
public class RefundResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RefundId { get; set; }
    public decimal RefundedAmount { get; set; }

    public static RefundResult Successful(string refundId, decimal amount)
    {
        return new RefundResult
        {
            Success = true,
            RefundId = refundId,
            RefundedAmount = amount
        };
    }

    public static RefundResult Failed(string errorMessage)
    {
        return new RefundResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}


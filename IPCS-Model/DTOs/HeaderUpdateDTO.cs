namespace IPCS_Model.DTOs
{
    /// <summary>
    /// Header-only update for purchase/sales/return documents.
    /// Only remarks/reason and the document date may change — quantities,
    /// amounts, parties and line items are NEVER editable through this DTO,
    /// so a header edit cannot corrupt stock or books.
    /// Null fields are left unchanged.
    /// </summary>
    public class DocumentHeaderUpdateDTO
    {
        public string? Remarks { get; set; }
        public string? Reason { get; set; }
        public DateTime? DocumentDate { get; set; }
    }

    /// <summary>
    /// Header-only update for a payment record.
    /// Only the remarks and transaction reference may change — amount,
    /// method and invoice links are NEVER editable through this DTO.
    /// Null fields are left unchanged.
    /// </summary>
    public class PaymentHeaderUpdateDTO
    {
        public string? Remarks { get; set; }
        public string? TransactionNo { get; set; }
    }
}

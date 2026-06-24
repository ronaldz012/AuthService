namespace sales.Contracts.dtos;
public class OpenCashRegisterDto
{
    public Guid BranchId { get; set; }
    public decimal OpeningBalance { get; set; }
}
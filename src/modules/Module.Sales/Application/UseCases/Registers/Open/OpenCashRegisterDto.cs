namespace Module.Sales.Application.UseCases.Registers.Open;
public class OpenCashRegisterDto
{
    public Guid BranchId { get; set; }
    public decimal OpeningBalance { get; set; }
}
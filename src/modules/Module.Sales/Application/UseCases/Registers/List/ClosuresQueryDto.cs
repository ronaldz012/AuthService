using Common.Utilities;

namespace Module.Sales.Application.UseCases.Registers.List;

public class ClosuresQueryDto : PaginationQueryDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

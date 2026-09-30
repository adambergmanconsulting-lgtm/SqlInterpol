namespace SqlInterpol.Testing.Specifications;

public abstract partial class FromTestSuite
{
    public record OrderLine(int OrderId, int ProductItemNumber, int Quantity, decimal Price, int ProductId);
}
using System.Numerics;

namespace Interfaces
{
    public interface IFacade
    {
        ISymbolicExpression NegativeInfinity { get; }
        ISymbolicExpression SymbolicVariable(string symbol);
        ISymbolicExpression SymbolicValue(double value);
        double MultiGammaLn(double a, double dimensionality);
        Complex[] FindPolynomialRoots(IEnumerable<double> coefficients);
        IMatrix VStack(List<IMatrix> matricesToStack);
        IMatrix VStack(IMatrix matrixA, IMatrix matrixB);
    }
}

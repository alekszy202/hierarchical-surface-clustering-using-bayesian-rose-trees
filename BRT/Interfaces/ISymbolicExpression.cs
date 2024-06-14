using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface ISymbolicExpression
    {
        bool IsNegativeInfinity { get; }
        ISymbolicExpression OneMinusSymbol { get; }
        ISymbolicExpression Numerator { get; }
        ISymbolicExpression Denominator { get; }
        ISymbolicExpression Add(double addend);
        ISymbolicExpression Subtract(double subtrahend);
        ISymbolicExpression Subtract(ISymbolicExpression subtrahend);
        ISymbolicExpression Multiply(double factor);
        ISymbolicExpression Multiply(ISymbolicExpression factor);
        ISymbolicExpression Divide(double divisor);
        ISymbolicExpression Divide(ISymbolicExpression divisor);
        ISymbolicExpression Power(int exponent);
        IEnumerable<double> PolynomialCoefficients(ISymbolicExpression symbolicVariable);
        double Evaluate(string symbol, double argument);
    }
}

using Interfaces;
using MathNet.Symbolics;

namespace MathNetFacade
{
    internal class SymbolicExpression : ISymbolicExpression
    {
        public MathNet.Symbolics.SymbolicExpression SymbolicExpressionData;

        public SymbolicExpression(MathNet.Symbolics.SymbolicExpression symbolicExpressionData)
        {
            SymbolicExpressionData = symbolicExpressionData;
        }

        public ISymbolicExpression OneMinusSymbol => new SymbolicExpression(1 - SymbolicExpressionData);

        public bool IsNegativeInfinity =>
            SymbolicExpressionData == MathNet.Symbolics.SymbolicExpression.NegativeInfinity;

        public ISymbolicExpression Numerator => new SymbolicExpression(SymbolicExpressionData.Numerator());

        public ISymbolicExpression Denominator => new SymbolicExpression(SymbolicExpressionData.Denominator());

        public IEnumerable<double> PolynomialCoefficients(ISymbolicExpression symbolicVariable)
        {
            SymbolicExpression variableSymbolicExpression = symbolicVariable as SymbolicExpression ?? throw new ArgumentException("Two expressions use different implementations");
            MathNet.Symbolics.SymbolicExpression[] coefficients = SymbolicExpressionData.Coefficients(variableSymbolicExpression.SymbolicExpressionData);
            return coefficients.Select(symbolicCoefficient => 
                symbolicCoefficient.Expression.IsPositiveInfinity ? double.PositiveInfinity : 
                    symbolicCoefficient.Expression.IsNegativeInfinity ? double.NegativeInfinity :
                    symbolicCoefficient.RealNumberValue);
        }

        public ISymbolicExpression Add(double addend)
        {
            return new SymbolicExpression(SymbolicExpressionData + addend);
        }
        public ISymbolicExpression Subtract(double subtrahend)
        {
            return new SymbolicExpression(SymbolicExpressionData - subtrahend);
        }

        public ISymbolicExpression Subtract(ISymbolicExpression subtrahend)
        {
            SymbolicExpression subtrahendSymbolicExpression = subtrahend as SymbolicExpression ?? throw new ArgumentException("Two expressions use different implementations");
            return new SymbolicExpression(SymbolicExpressionData - subtrahendSymbolicExpression.SymbolicExpressionData);
        }

        public ISymbolicExpression Multiply(double factor)
        {
            return new SymbolicExpression(SymbolicExpressionData * factor);
        }

        public ISymbolicExpression Multiply(ISymbolicExpression factor)
        {
            SymbolicExpression factorSymbolicExpression = factor as SymbolicExpression ?? throw new ArgumentException("Two expressions use different implementations");
            return new SymbolicExpression(SymbolicExpressionData * factorSymbolicExpression.SymbolicExpressionData);
        }

        public ISymbolicExpression Divide(double divisor)
        {
            return new SymbolicExpression(SymbolicExpressionData / divisor);
        }

        public ISymbolicExpression Divide(ISymbolicExpression divisor)
        {
            SymbolicExpression divisorSymbolicExpression = divisor as SymbolicExpression ?? throw new ArgumentException("Two expressions use different implementations");
            return new SymbolicExpression(SymbolicExpressionData / divisorSymbolicExpression.SymbolicExpressionData);
        }

        public ISymbolicExpression Power(int exponent)
        {
            return new SymbolicExpression(SymbolicExpressionData.Pow(exponent));
        }

        public double Evaluate(string symbol, double argument)
        {
            if (SymbolicExpressionData.Expression.IsPositiveInfinity)
                return double.PositiveInfinity;
            else if (SymbolicExpressionData.Expression.IsNegativeInfinity)
                return double.NegativeInfinity;
            else
                return SymbolicExpressionData.Evaluate(new Dictionary<string, FloatingPoint>() { {symbol, argument} }).RealValue;
        }
    }
}

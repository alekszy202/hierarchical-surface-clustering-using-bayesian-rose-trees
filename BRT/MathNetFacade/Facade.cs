using System;
using System.Numerics;
using Interfaces;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace MathNetFacade
{
    public class Facade : IFacade
    {
        private static readonly double LogPi = Math.Log(Math.PI);
        private static Facade? _facade;

        public static IFacade Instance => _facade ??= new Facade();

        public ISymbolicExpression NegativeInfinity => new SymbolicExpression(MathNet.Symbolics.SymbolicExpression.NegativeInfinity);

        private Facade()
        {

        }

        public ISymbolicExpression SymbolicVariable(string symbol)
        {
            return new SymbolicExpression(MathNet.Symbolics.SymbolicExpression.Variable(symbol));
        }

        public ISymbolicExpression SymbolicValue(double value)
        {
            return new SymbolicExpression(new MathNet.Symbolics.SymbolicExpression(value));
        }

        public double MultiGammaLn(double a, double dimensionality)
        {
            double accumulator = dimensionality * (dimensionality - 1) / 4 * LogPi;
            for (int i = 0; i < dimensionality; ++i)
            {
                accumulator += SpecialFunctions.GammaLn(a - 0.5 * i);
            }

            return accumulator;
        }

        public Complex[] FindPolynomialRoots(IEnumerable<double> coefficients)
        {

            double[] coeffs = coefficients.ToArray();
            int lastNonZero = coeffs.Length - 1;
            for (int degree = lastNonZero; degree > 0; degree--)
            {
                if (coeffs[degree] != 0)
                {
                    lastNonZero = degree;
                    break;
                }

            }

            coeffs = coeffs[0..(lastNonZero + 1)];
            try
            {
                Complex[] roots = FindRoots.Polynomial(coeffs);
                return roots;
            }
            catch (NonConvergenceException e)
            {
                Console.WriteLine(e);
                return Array.Empty<Complex>();
            }
        }
        //TODO: VSTack nie wiem w jakiej kolejności składa koniec końców te macierze, co znaczy lower?
        public IMatrix VStack(List<IMatrix> matricesToStack)
        {
            IEnumerable<Matrix<double>> matrices = matricesToStack.Select(m =>
                (m as MNMatrix ?? throw new ArgumentException("Matrix uses different implementation")).MatrixData);

            int colCount = matrices.Any() ? matrices.ElementAt(0).ColumnCount : 0;
            Matrix<double> result = Matrix<double>.Build.Dense(0, colCount, 0.0);
            foreach (Matrix<double> matrix in matrices)
            {
                result = matrix.Stack(result);
            }

            return new MNMatrix(result);
        }

        public IMatrix VStack(IMatrix matrixA, IMatrix matrixB)
        {
            Matrix<double> matA =
                (matrixA as MNMatrix ?? throw new ArgumentException("Matrix uses different implementation")).MatrixData;
            Matrix<double> matB =
                (matrixB as MNMatrix ?? throw new ArgumentException("Matrix uses different implementation")).MatrixData;
            if (matA.ColumnCount == 0)
                return new MNMatrix(matB.Clone());
            if (matB.ColumnCount == 0)
                return new MNMatrix(matA.Clone());
            return new MNMatrix(matB.Stack(matA));
        }

        public static double CalculateStandardDeviation(double[] numbers)
        {
            if (numbers == null || numbers.Length == 0) return 0;

            double sum = 0;
            foreach (double number in numbers)
            {
                sum += number;
            }

            double mean = sum / numbers.Length;

            double varianceSum = 0;
            foreach (double number in numbers)
            {
                varianceSum += (number - mean) * (number - mean);
            }

            double variance = varianceSum / numbers.Length;
            return Math.Sqrt(variance);
        }
    }
}

using Interfaces;

namespace BRT
{
    /// <summary>
    /// Reference: MURPHY, Kevin P.
    /// Conjugate Bayesian analysis of the Gaussian distribution.
    /// def, v. 1, n. 2σ2, p. 16, 2007.
    /// https://www.cse.iitk.ac.in/users/piyush/courses/tpmi_winter19/readings/bayesGauss.pdf
    /// </summary>
    public class NormalInverseWishart : IModel
    {
        private static readonly double Ln2 = Math.Log(2);
        private static readonly double Ln2Pi = Math.Log(2 * Math.PI);
        private readonly IFacade _facade;
        private readonly double _scaleFactor;
        private readonly int _degreesOfFreedom;
        private readonly IVector _dataMean;
        private readonly IMatrix _scatterMatrix;
        private readonly double _lnPrior0;
        private readonly double _prior0;

        /// <summary>
        /// Instantiates a model of normal-inverse-Wishart distribution
        /// </summary>
        /// <param name="facade">Facade of math library.</param>
        /// <param name="data">Data to be modeled using NIW distribution. First index corresponds to sample.</param>
        /// <param name="varianceRatio">A ratio of expected variance in a cluster in relation to variance in data</param>
        /// <param name="scaleFactor">Number of prior measurements on the scale of Σ</param>
        public NormalInverseWishart(IFacade facade, IMatrix data, double varianceRatio, double scaleFactor)
        {
            _facade = facade;
            _scaleFactor = scaleFactor;

            //W bayesGauss: ν
            //Dla odwrotnego rozkładu wisharta ν > d-1, gdzie macierz kowariancji X ma d na d.
            //Dla jednego wymiaru, nasza macierz kowariancji to 1x1, czyli d=1. d-1=0, więc ν musi być > 0.
            //Nie wiem czemu dof jest zawsze o 1 1 większy niż d. Ale tak jest tutaj w tej implementacji.
            _degreesOfFreedom = data.SampleDimensionality + 1;

            //Średnia wartość w macierzy danych
            _dataMean = data.Mean;

            //dataCovarianceMatrix jest macierzą kowariancji. Dla jednowymiarowych danych to pojedyncza liczba = wariancja
            IMatrix dataCovarianceMatrix = data.Transposed.CovarianceMatrix;

            //U Caponettiego varianceRatio to g
            //scatter matrix jest tu chyba macierzą S z bayesgauss. W tym przypadku jest to wariancja podzielone przez g. Co ona symbolizuje?
            _scatterMatrix = dataCovarianceMatrix.Divide(varianceRatio).Transposed;

            _lnPrior0 = CalculateLnPrior(_scatterMatrix, _scaleFactor, _degreesOfFreedom);
            //Na potrzeby BRF, alternatywnie z _lnPrior0
            _prior0 = CalculatePrior(_scatterMatrix, _scaleFactor, _degreesOfFreedom);
        }

        private double CalculateLnPrior(IMatrix scatterMatrix, double scaleFactor, int degreesOfFreedom)
        {
            int dimensionality = scatterMatrix.SampleDimensionality;
            double logPrior = Ln2 * (degreesOfFreedom * dimensionality * 0.5) +
                              (dimensionality * 0.5) * Math.Log(2 * Math.PI / scaleFactor);
            double determinant = scatterMatrix.Determinant;

            if (determinant <= 0)
                logPrior = double.NegativeInfinity;
            else
                logPrior += _facade.MultiGammaLn(degreesOfFreedom * 0.5, dimensionality) -
                            0.5 * degreesOfFreedom * Math.Log(determinant);
            return logPrior;
        }

        private double CalculatePrior(IMatrix scatterMatrix, double scaleFactor, int degreesOfFreedom)
        {
            int dimensionality = scatterMatrix.SampleDimensionality;

            // (249) z bayesgauss
            double prior = Math.Pow(2, degreesOfFreedom * dimensionality * 0.5) *
                           Math.Pow(2.0 * Math.PI / scaleFactor, dimensionality * 0.5);

            // Da się to zrobić wydajniej
            prior *= Math.Exp(_facade.MultiGammaLn(degreesOfFreedom * 0.5, dimensionality)) /
                     Math.Pow(scatterMatrix.Determinant, degreesOfFreedom * 0.5);
            return prior;
        }

        private (IMatrix posteriorScatterMatrix, double posteriorScaleFactor, int posteriorDegreesOfFreedom) CalculatePosterior(IMatrix samples)
        {
            int sampleCount = samples.SampleCount;
            IVector meanSample = samples.Mean;
            double posteriorScaleFactor = _scaleFactor + sampleCount;
            int posteriorDegreesOfFreedom = _degreesOfFreedom + sampleCount;
            IMatrix posteriorScatterMatrixTransposed = samples.Transposed.CovarianceMatrix.Multiply(sampleCount - 1);
            IVector sampleMeanDifference = meanSample.Subtract(_dataMean);
            IMatrix posteriorScatterMatrix = _scatterMatrix.Add(posteriorScatterMatrixTransposed,
                sampleMeanDifference.OuterProduct().Multiply(_scaleFactor * sampleCount / posteriorScaleFactor));
            return (posteriorScatterMatrix, posteriorScaleFactor, posteriorDegreesOfFreedom);
        }

        public double CalculateMarginalLikelihood(IMatrix samples)
        {
            int sampleCount = samples.SampleCount;
            int sampleDimensionality = samples.SampleDimensionality;
            (IMatrix posteriorScatterMatrix, double posteriorScaleFactor, int posteriorDegreesOfFreedom) = CalculatePosterior(samples);
            double prior = CalculatePrior(posteriorScatterMatrix, posteriorScaleFactor, posteriorDegreesOfFreedom);
            return prior / (_prior0 * Math.Pow(2 * Math.PI, sampleCount * sampleDimensionality * 0.5));
        }

        public double CalculateLnMarginalLikelihood(IMatrix samples)
        {
            int sampleCount = samples.SampleCount;
            int sampleDimensionality = samples.SampleDimensionality;
            (IMatrix posteriorScatterMatrix, double posteriorScaleFactor, int posteriorDegreesOfFreedom) = CalculatePosterior(samples);
            double lnPrior = CalculateLnPrior(posteriorScatterMatrix, posteriorScaleFactor, posteriorDegreesOfFreedom);
            return lnPrior - _lnPrior0 - sampleCount * sampleDimensionality * 0.5 * Ln2Pi;
        }
    }
}
using System;

namespace BetaSeriesW8.Service
{
    public class BetaSerieException : Exception
    {
        public BetaSerieException(int codeErreur) : base(TraductionErreur.RecupererErreur(codeErreur))
        {
        }
    }
}
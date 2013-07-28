using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingWizard.StrategyAnalysis.OptionLib
{
    public class OptionCalculator
    {
        public decimal CalcualtePrice(Option opt, decimal equityPrice, decimal interest, 
            DateTime calculationDate, double volatility)
        {
            double t = opt.ExpiryDate.Subtract(calculationDate).Days / 365.0;

            double d1 = (Math.Log((double)(equityPrice / opt.Strike), Math.E)
                + ((double)(interest) + volatility * volatility / 2) * t) / (volatility * Math.Sqrt(t));
            double d2 = d1 - volatility * Math.Sqrt(t);

            double callDelta = NormalDistribution(d1);
            double callPrice = (double)equityPrice * NormalDistribution(d1) -
                (double)opt.Strike * Math.Exp(-1 * (double)interest * t) * NormalDistribution(d2);

            if (opt.IsCall)
            {
                return (decimal)callPrice;
            }
            else
            {
                double putPrice = callPrice + (double)opt.Strike - (double)equityPrice - (t * (double)(opt.Strike * interest));

                return (decimal)putPrice;
            }
        }

        private static double NormalDistribution(double d)
        {
            double y = 1 / (1 + .2316419 * Math.Abs(d));
            double z = .3989423 * Math.Exp(-1 * d * d / 2);
            double x = 1 - z * (1.330274 * Math.Pow(y, 5) - 1.821256 * Math.Pow(y, 4) +
                1.781478 * Math.Pow(y, 3) - .356538 * Math.Pow(y, 2) + .3193815 * y);

            if (d > 0)
                return x;
            else
                return 1 - x;
        }

        public PLSerious CalculatePLs(OptionCombination optComb, DateTime calculationDate, 
            decimal lowPrice, decimal highPrice, decimal interest, double volatility)
        {
            PLSerious plSerious = new PLSerious();
            plSerious.CalculationDate = calculationDate;
            plSerious.Volatility = volatility;

            decimal interval = (highPrice - lowPrice) / 30;

            decimal equityPrice = lowPrice;

            while (equityPrice < highPrice+interval)
            {
                PLPoint plPoint = new PLPoint();
                plPoint.EquityPrice = equityPrice;

                foreach (OpenOption openOpt in optComb)
                {
                    decimal optPrice = CalcualtePrice(openOpt.Option, equityPrice, interest, calculationDate, volatility);

                    plPoint.Profit += (optPrice - openOpt.PurchasePrice) * openOpt.ContractNo *100;
                }

                plSerious.PLPointSerious.Add(plPoint);

                equityPrice += interval;
            }

            return plSerious;
        }
    }
}

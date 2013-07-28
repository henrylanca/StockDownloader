using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingWizard.StrategyAnalysis.OptionLib
{
    public class PLPoint
    {
        public decimal EquityPrice { get; set; }
        public decimal Profit { get; set; }
    }

    public class PLSerious
    {
        public DateTime CalculationDate { get; set; }
        public double Volatility { get; set; }
        public List<PLPoint> PLPointSerious { get; private set; }

        public PLSerious()
        {
            PLPointSerious = new List<PLPoint>();
        }
    }
}

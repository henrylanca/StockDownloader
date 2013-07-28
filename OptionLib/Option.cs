using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingWizard.StrategyAnalysis.OptionLib
{
    public class Option
    {
        public string Symbol { get; set; }

        public DateTime ExpiryDate { get; set; }

        public decimal Strike { get; set; }

        public bool IsCall { get; set; }

    }

    public class OpenOption
    {
        public Option Option { get; set; }

        public DateTime PurchaseDate { get; set; }

        public int ContractNo { get; set; }

        public decimal PurchasePrice { get; set; }
    }

    public class OptionCombination : IEnumerable<OpenOption>
    {
        public List<OpenOption> Options { get; private set; }

        public OptionCombination()
        {
            this.Options = new List<OpenOption>();
        }

        public IEnumerator<OpenOption> GetEnumerator()
        {
            return Options.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
}
}

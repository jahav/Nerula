using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nerula.Data
{
    public class Wip : EntityBase
    {
        public Wip()
        {
            PriceComponents = new HashSet<WipPriceComponent>();
        }

        public virtual Project Project { get; set; }
        public virtual ISet<WipPriceComponent> PriceComponents { get; set; }
        public virtual WipState State {get;set;}

        public virtual DateTime Period { get; set; }
    }
}

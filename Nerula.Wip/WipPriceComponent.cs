using System;

namespace Nerula.Data
{
    public class WipPriceComponent : EntityBase
    {
        public virtual Wip Wip { get; set; }
        public virtual decimal EstWorkload { get; set; }
        public virtual decimal SoldWorkload { get; set; }
        public virtual WipComponentType Type { get; set; }
    }
}

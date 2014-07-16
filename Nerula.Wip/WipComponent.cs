using System;

namespace Nerula.Data
{
    public class WipComponent : Nerula.Data.EntityBase
    {
        public virtual decimal Cost { get; set; }
        public virtual decimal Workload { get; set; }
        public virtual string Comment { get; set; }
    }
}

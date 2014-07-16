using System;

namespace Nerula.Data
{
    public class WipSubcontract : Nerula.Data.EntityBase
    {
        public virtual decimal Cost { get; set; }
        public virtual string Note { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nerula.Data
{
    public class Project : EntityBase
    {
        public virtual string Code { get; set; }
        public virtual DateTime StartDate { get; set; }
    }
}

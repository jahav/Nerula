using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nerula.Data
{
	[Serializable]
	public class Allocation
	{
		// Notice lack of Id - it is present in database, but not in component
		public virtual Invoice Invoice { get; set; }
		public virtual Conjecture Conjecture { get; set; }
		public virtual decimal Amount { get; set; }
		public virtual DateTime UpdatedDate {get;set;}
		public virtual string UpdaterName{get;set;}
	}
}

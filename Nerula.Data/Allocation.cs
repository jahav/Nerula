using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nerula.Data
{
	public class Allocation
	{
		public virtual int AllocationId { get; set; }
		public virtual Invoice Invoice { get; set; }
		public virtual Conjecture Conjecture { get; set; }
		public virtual decimal Amount { get; set; }
		public virtual DateTime UpdatedDate {get;set;}
		public virtual string UpdaterName{get;set;}
	}
}

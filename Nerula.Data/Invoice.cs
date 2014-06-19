using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nerula.Data
{
	public class Invoice
	{
		public Invoice()
		{
			Allocations = new List<Allocation>();
		}

		public virtual int InvoiceId { get; protected set; }
		public virtual string Number { get; set; }

		public virtual IList<Allocation> Allocations { get; set; }

		public virtual IList<Conjecture> Conjectures { get; set; }

		public virtual Allocation AddAllocation(Conjecture conjecture, int allocatedAmount)
		{
			var allocation = new Allocation
			{
				Invoice = this,
				Conjecture = conjecture,
				Amount = allocatedAmount,
				UpdatedDate = DateTime.UtcNow,
				UpdaterName = "Honza"
			};
			this.Allocations.Add(allocation);
			return allocation;
		}
	}
}

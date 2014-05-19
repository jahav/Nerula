using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nerula.Data
{
	public class Conjecture
	{
		/// <summary>
		/// NHibernate constructor.
		/// </summary>
		protected Conjecture()
		{
		}

		public Conjecture(string name, int amount)
		{
			this.Name = name;
			this.Amount = amount;
		}
		public virtual int ConjectureId { get; protected set; }
		public virtual string Name { get; set; }
		public virtual int Amount { get; set; }
	}
}

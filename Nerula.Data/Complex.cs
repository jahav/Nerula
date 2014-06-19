using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nerula.Data
{
	public class Complex
	{
		public virtual int ComplexId { get; set; }
		public virtual MyComponent Component { get; set; }
		public virtual string OtherProperty { get; set; }
	}

}

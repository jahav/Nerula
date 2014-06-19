using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nerula.Data
{
	public class Post : EntityBase
	{
		public virtual string Title { get; set; }
		public virtual string Body { get; set; }
		public virtual Blog Blog { get; set; }

		public virtual bool Remove()
		{
			return Blog.Posts.Remove(this);
		}
	}
}

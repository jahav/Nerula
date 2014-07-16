using System;

namespace Nerula.Data
{
	public class Post : EntityBase
	{
		public Post(int id) : base(id) { }

		/// <summary>
		/// Ctor for NH.
		/// </summary>
		public Post() { }

		public virtual string Title { get; set; }
		public virtual string Body { get; set; }
		public virtual Blog Blog { get; set; }

		public virtual bool Remove()
		{
			return Blog.Posts.Remove(this);
		}
	}
}

using System;
using System.Collections.Generic;

namespace Nerula.Data
{
	public class Blog : EntityBase
	{
		/// <summary>
		/// Ctor for NH.
		/// </summary>
		public Blog()
		{
			Posts = new HashSet<Post>();
		}

		public Blog(int id)
			: base(id)
		{
			Posts = new HashSet<Post>();
		}

		public virtual string Name { get; set; }

		// This would be configured to lazy-load.
		public virtual ISet<Post> Posts { get; set; }

		public virtual Post AddPost(string title, string body)
		{
			var post = new Post() { Title = title, Body = body, Blog = this };
			Posts.Add(post);
			return post;
		}
	}
}

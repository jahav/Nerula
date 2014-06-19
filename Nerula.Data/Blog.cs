using System;
using System.Collections.Generic;

namespace Nerula.Data
{
	public class Blog : EntityBase
	{
		public virtual string Name { get; set; }

		// This would be configured to lazy-load.
		public virtual ISet<Post> Posts { get; protected set; }

		public Blog()
		{
			Posts = new HashSet<Post>();
		}

		public virtual Post AddPost(string title, string body)
		{
			var post = new Post() { Title = title, Body = body, Blog = this };
			Posts.Add(post);
			return post;
		}
	}
}

using System;

namespace Nerula.Data
{
	public abstract class EntityBase
	{
		public virtual int Id { get; protected set; }

		protected bool IsTransient { get { return Id == 0; } }

		public override bool Equals(object obj)
		{
			return EntityEquals(obj as EntityBase);
		}

		protected bool EntityEquals(EntityBase other)
		{
			if (other == null || !GetType().IsInstanceOfType(other))
			{
				return false;
			}
			// One entity is transient and the other is persistent.
			else if (IsTransient ^ other.IsTransient)
			{
				return false;
			}
			// Both entities are not saved.
			else if (IsTransient && other.IsTransient)
			{
				return ReferenceEquals(this, other);
			}
			else
			{
				// Compare transient instances.
				return Id == other.Id;
			}
		}

		// The hash code is cached because a requirement of a hash code is that
		// it does not change once calculated. For example, if this entity was
		// added to a hashed collection when transient and then saved, we need
		// the same hash code or else it could get lost because it would no 
		// longer live in the same bin.
		private int? cachedHashCode;

		public override int GetHashCode()
		{
			if (cachedHashCode.HasValue) return cachedHashCode.Value;

			cachedHashCode = IsTransient ? base.GetHashCode() : Id.GetHashCode();
			return cachedHashCode.Value;
		}

		// Maintain equality operator semantics for entities.
		public static bool operator ==(EntityBase x, EntityBase y)
		{
			// By default, == and Equals compares references. In order to 
			// maintain these semantics with entities, we need to compare by 
			// identity value. The Equals(x, y) override is used to guard 
			// against null values; it then calls EntityEquals().
			return Object.Equals(x, y);
		}

		// Maintain inequality operator semantics for entities. 
		public static bool operator !=(EntityBase x, EntityBase y)
		{
			return !(x == y);
		}
	}
}

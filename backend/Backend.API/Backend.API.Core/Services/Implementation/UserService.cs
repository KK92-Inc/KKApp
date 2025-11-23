using System.Linq.Expressions;
using Backend.API.Domain;

namespace Backend.API.Core.Services.Implementation;

public class UserService<T>() where T : BaseEntity
{
	// Ensure your Interface matches this signature too!
	public virtual async Task<List<T>> GetAllAsync(

		// The ? here is the magic fix
		params Expression<Func<T, bool>>?[] filters)
	{
		var query = _dbSet.AsQueryable();

		foreach (var filter in filters)
		{
			// Now the compiler knows 'filter' might be null, so we check it.
			if (filter != null)
			{
				query = query.Where(filter);
			}
		}

		return await query.ToPagedListAsync(p);
	}

	public Expression<Func<T, bool>>? With<TVal>(TVal? value, Expression<Func<T, bool>> filter)
	{
		if (value is null) return null;

		// Special handling for strings to ignore Empty strings too
		if (value is string s && string.IsNullOrEmpty(s)) return null;

		return filter;
	}
}
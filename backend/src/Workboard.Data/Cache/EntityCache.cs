using System.Collections.Concurrent;

namespace Workboard.Data.Cache
{
    /// <summary>
    /// In-memory, write-through cache for one entity type: items by id plus a per-parent index.
    /// Assumes a single API process owns the database; there is no expiry, so edits made to the SQLite
    /// file behind the cache's back are only seen after a restart (or <see cref="Clear"/>).
    /// Hot reads by id are lock-free; index reads and every write take <see cref="Gate"/>.
    /// Lock order is downward only (workspace, project, column, task): a cache may call a cache below it
    /// while holding its own lock, but must read a cache above it (e.g. a task reading its column) before taking its own.
    /// </summary>
    public abstract class EntityCache<T>(Func<T, Guid> idOf, Func<T, Guid> parentOf, IComparer<T>? order = null) where T : class
    {
        private readonly ConcurrentDictionary<Guid, T> items = new();
        private readonly HashSet<Guid> loadedParents = [];

        protected object Gate { get; } = new();

        /// <summary>Cache hit, or <paramref name="load"/> from the store on a miss. Misses that find nothing are not cached.</summary>
        protected T? GetOrLoad(Guid id, Func<Guid, T?> load)
        {
            if (items.TryGetValue(id, out T? hit))
            {
                return hit;
            }

            lock (Gate)
            {
                if (items.TryGetValue(id, out hit))
                {
                    return hit;
                }
                T? loaded = load(id);
                if (loaded is not null)
                {
                    items[idOf(loaded)] = loaded;
                }
                return loaded;
            }
        }

        /// <summary>All children of a parent: one store query the first time, memory afterwards.</summary>
        protected List<T> GetChildren(Guid parentId, Func<Guid, IEnumerable<T>> load)
        {
            lock (Gate)
            {
                if (loadedParents.Add(parentId))
                {
                    try
                    {
                        foreach (T item in load(parentId))
                        {
                            items[idOf(item)] = item;
                        }
                    }
                    catch
                    {
                        loadedParents.Remove(parentId);
                        throw;
                    }
                }

                List<T> children = [.. items.Values.Where(i => parentOf(i) == parentId)];
                children.Sort(order);
                return children;
            }
        }

        protected void Put(T item)
        {
            lock (Gate)
            {
                items[idOf(item)] = item;
            }
        }

        protected void Remove(Guid id) => items.TryRemove(id, out _);

        /// <summary>Drops every cached child of a parent, e.g. after the parent was deleted in the database.</summary>
        internal virtual void EvictByParent(Guid parentId)
        {
            lock (Gate)
            {
                foreach (T item in items.Values.Where(i => parentOf(i) == parentId))
                {
                    items.TryRemove(idOf(item), out _);
                }
                loadedParents.Remove(parentId);
            }
        }

        public virtual void Clear()
        {
            lock (Gate)
            {
                items.Clear();
                loadedParents.Clear();
            }
        }
    }
}

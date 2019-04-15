using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FleetTrack.Core.Common;
using FleetTrack.Core.Domain;
using FleetTrack.Core.Interfaces;

namespace FleetTrack.Infrastructure.Persistence
{
    public class InMemoryRepository<T> : IRepository<T> where T : Entity
    {
        private readonly ConcurrentDictionary<Guid, T> _items = new ConcurrentDictionary<Guid, T>();

        protected IEnumerable<T> Items => _items.Values;

        public virtual T Get(Guid id)
        {
            T item;
            return _items.TryGetValue(id, out item) ? item : null;
        }

        public virtual IReadOnlyList<T> GetAll()
        {
            return _items.Values.ToList();
        }

        public virtual void Add(T entity)
        {
            Guard.NotNull(entity, nameof(entity));

            if (!_items.TryAdd(entity.Id, entity))
            {
                throw new InvalidOperationException($"Entity {entity.Id} already exists.");
            }
        }

        public virtual void Update(T entity)
        {
            Guard.NotNull(entity, nameof(entity));

            if (!_items.ContainsKey(entity.Id))
            {
                throw new KeyNotFoundException($"Entity {entity.Id} does not exist.");
            }

            _items[entity.Id] = entity;
        }

        public virtual bool Remove(Guid id)
        {
            T removed;
            return _items.TryRemove(id, out removed);
        }
    }
}

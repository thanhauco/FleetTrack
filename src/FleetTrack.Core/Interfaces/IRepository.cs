using System;
using System.Collections.Generic;
using FleetTrack.Core.Domain;

namespace FleetTrack.Core.Interfaces
{
    public interface IRepository<T> where T : Entity
    {
        T Get(Guid id);

        IReadOnlyList<T> GetAll();

        void Add(T entity);

        void Update(T entity);

        bool Remove(Guid id);
    }
}

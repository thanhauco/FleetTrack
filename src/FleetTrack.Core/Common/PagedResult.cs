using System;
using System.Collections.Generic;
using System.Linq;

namespace FleetTrack.Core.Common
{
    public class PagedResult<T>
    {
        public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
        {
            Items = items;
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        public IReadOnlyList<T> Items { get; }

        public int Page { get; }

        public int PageSize { get; }

        public int TotalCount { get; }

        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasNext => Page < TotalPages;

        public static PagedResult<T> Create(IEnumerable<T> source, int page, int pageSize)
        {
            Guard.NotNull(source, nameof(source));
            Guard.Positive(page, nameof(page));
            Guard.Positive(pageSize, nameof(pageSize));

            var all = source.ToList();
            var items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return new PagedResult<T>(items, page, pageSize, all.Count);
        }
    }
}

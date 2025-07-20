using Dallal_Backend_v2.Entities;

namespace Dallal_Backend_v2.Repositories;

public record struct ItemsAndTotalCount<T>(List<T> Items, long Count)
    where T : BaseEntity { }

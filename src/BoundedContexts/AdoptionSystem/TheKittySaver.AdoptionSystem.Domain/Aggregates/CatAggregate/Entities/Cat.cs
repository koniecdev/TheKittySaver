using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;

public sealed class Cat : AggregateRoot<CatId>
{
    public CatName Name { get; private set; }
    public CatAge Age { get; private set; }
    public bool Sex { get; private set; }

    public static Cat Create(
        CatName name,
        CatAge age)
    {
        CatId id = CatId.New();
        var instance = new Cat(id, name, age);
        return instance;
    }
    
    private Cat(
        CatId id,
        CatName name,
        CatAge age) : base(id)
    {
        Name = name;
        Age = age;
    }
}
namespace CraftFlow.SharedKernel.Domain;

public abstract class Entity
{
    public Guid Id { get; protected set; }
    public bool IsActive { get; protected set; } = true;

    protected Entity(Guid id)
    {
        Id = id;
    }

    protected Entity() { }

    public virtual void Archive()
    {
        IsActive = false;
    }

    public virtual void Restore()
    {
        IsActive = true;
    }
}
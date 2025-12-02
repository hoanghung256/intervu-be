namespace Intervu.Domain.Abstractions.Entity.Interfaces
{
    public interface IEntityBase<T>
    {
        public T Id { get; set; }
    }
}

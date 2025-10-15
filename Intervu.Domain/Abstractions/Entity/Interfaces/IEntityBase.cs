namespace Intervu.Domain.Abstractions.Entities.Interfaces
{
    public interface IEntityBase<T>
    {
        public T Id { get; set; }
    }
}

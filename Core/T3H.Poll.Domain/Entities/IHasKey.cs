namespace T3H.Poll.Domain.Entities;

public interface IHasKey<T>
{
    T Id { get; set; }
}

namespace GameServer.Model.IoC;


public abstract class BaseSystem
{
    public virtual void Initialize()
    {
        Console.WriteLine($"{GetType().Name} initialized");
    }
}
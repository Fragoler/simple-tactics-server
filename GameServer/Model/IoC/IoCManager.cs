using System.Reflection;

namespace GameServer.Model.IoC;


public sealed class IoCManager
{
    private readonly Dictionary<Type, BaseSystem> _systems = new();

    public IoCManager()
    {
        AutoRegisterSystems();
        InitializeAll();
    }
    
    public void AutoRegisterSystems()
    {
        var systemTypes = FindDerivedTypes(Assembly.GetExecutingAssembly(), typeof(BaseSystem));

        foreach (var type in systemTypes)
        {
            if (type.IsAbstract)
                continue;
                        
            Register(type);
        }
    }
    

    public T Resolve<T>() where T : BaseSystem
    {
        if (_systems.TryGetValue(typeof(T), out var system))
        {
            return (T) system;
        }

        throw new InvalidOperationException($"System {typeof(T).Name} is not registered");
    }
    
    public void Register<T>() where T : BaseSystem, new()
    {
        Register(typeof(T));
    }
    
    private void Register(Type type)
    {
        if (_systems.ContainsKey(type))
            throw new InvalidOperationException($"System {type.Name} is already registered");

        if (Activator.CreateInstance(type) is BaseSystem system)
        {
            _systems.Add(type, system);
        }
        else
            throw new OutOfMemoryException($"Error creating system type: {type.Name}");
    }
    
    public void InitializeAll()
    {
        foreach (var system in _systems.Values)
            InjectDependencies(system);
        
        foreach (var system in _systems.Values) 
            system.Initialize();
    }

    private void InjectDependencies(BaseSystem system)
    {
        var type = system.GetType();
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        foreach (var field in fields)
        {
            if (field.GetCustomAttribute<DependencyAttribute>() is null)
                continue;
    
            if (_systems.TryGetValue(field.FieldType, out var dependency))
                field.SetValue(system, dependency);
            else
                throw new InvalidOperationException(
                    $"Dependency {field.FieldType.Name} not found for {type.Name}");
        }
    }

    private static IEnumerable<Type> FindDerivedTypes(Assembly assembly, Type baseType)
    {
        return assembly.GetTypes().Where(t => baseType.IsAssignableFrom(t) && t != baseType);
    }
    
    private static IEnumerable<Type> GetClassesWithAttribute<TAttribute>(Assembly assembly) where TAttribute : Attribute
    {
        
        return assembly.GetTypes().Where(t => t.IsClass && t.GetCustomAttribute<TAttribute>() is not null);
    }
}
using System.Diagnostics.CodeAnalysis;
using GameServer.Model.Entities;
using GameServer.Model.EventBus;
using GameServer.Model.IoC;

namespace GameServer.Model.Components;


public class ComponentSystem : BaseSystem
{
    [Dependency] private EventBusSystem _event = null!;

    public override void Initialize()
    {
        base.Initialize();
        
        _event.SubscribeEntity<EntityRemoveEvent>(RemoveAllComponents);
    }

    private void RemoveAllComponents(Entity ent, BaseEntityEvent ev)
    {
        var componentTypes = ent.Info.Components.Keys.ToList();
        foreach (var compType in componentTypes)
            RemoveComponent(ent, compType);
    }

    public T? GetComponentOrDefault<T>(Entity ent) where T : Component
    {
        if (ent.Info.Components.TryGetValue(typeof(T), out var comp))
            return (T) comp;
        return null;
    }
    
    public bool TryGetComponent<T>(Entity ent, [NotNullWhen(true)] out T? comp) where T : Component
    {
        comp = GetComponentOrDefault<T>(ent);
        return comp != null;
    }
    
    public T EnsureComponent<T>(Entity ent) where T : Component
    {
        if (!TryGetComponent<T>(ent, out var comp))
            comp = AddComponent<T>(ent);
        return comp;
    }
    
    public T AddComponent<T>(Entity ent) where T : Component
    {
        var comp = Activator.CreateInstance<T>();
        
        if (!ent.Game.Components.TryGetValue(typeof(T), out var comps))
            ent.Game.Components.Add(typeof(T), comps = []);
        
        comps.Add(comp);
        ent.Info.Components.Add(typeof(T), comp);
        return comp;
    }

    public void RemoveComponent<T>(Entity ent) where T : Component
    {
        RemoveComponent(ent, typeof(T));
    }
    
    public void RemoveComponent(Entity ent, Type compType) 
    {
        if (ent.Game.Components.TryGetValue(compType, out var comps))
            comps.Remove(ent.Info.Components[compType]);

        ent.Info.Components.Remove(compType);
    }

    public bool HasComponent<T>(Entity ent) where T : Component
    {
        return HasComponent(ent, typeof(T));
    }
    
    public bool HasComponent(Entity ent, Type compType)
    {
        return ent.Info.Components.ContainsKey(compType);
    }
}
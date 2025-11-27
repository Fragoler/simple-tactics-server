using GameServer.Model.Components;
using GameServer.Model.Entities;
using GameServer.Model.IoC;

namespace GameServer.Model.EventBus;


public sealed class EventBusSystem : BaseSystem
{
    [Dependency] private readonly ComponentSystem _comp = null!;
    
    private readonly Dictionary<Type, List<Action<BaseEvent>>> _eventSub = new();
    private readonly Dictionary<Type, List<Action<Entity, BaseEntityEvent>>> _entityEventSub = new();
    private readonly Dictionary<Type, List<(Action<Entity, BaseEntityEvent> callback, Type compType)>> _entityEventCompSub = new();
    
    
    public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : BaseEvent
    {
        var type = typeof(TEvent);
        if (!_eventSub.TryGetValue(type, out var handlers))
            _eventSub[type] = handlers = [];
        handlers.Add((Action<BaseEvent>)handler);
    }
    
    public void SubscribeEntity<TEvent>(Action<Entity, TEvent> handler) 
        where TEvent : BaseEntityEvent
    {
        var eventType = typeof(TEvent);
        if (!_entityEventSub.ContainsKey(eventType))
            _entityEventSub[eventType] = [];
        
        _entityEventSub[eventType].Add(Wrapper);
        return;

        void Wrapper(Entity entity, BaseEntityEvent evt)
        {
            if (evt is TEvent typedEvent) handler(entity, typedEvent);
        }
    }
    
    public void SubscribeEntity<TEvent, TComp>(Action<Entity, TEvent> handler)
        where TEvent : BaseEntityEvent
        where TComp  : Component
    {
        var type = typeof(TEvent);
        if (!_entityEventCompSub.TryGetValue(type, out var handlers))
            _entityEventCompSub[type] = handlers = [];
        
        handlers.Add((Wrapper, typeof(TComp)));
        return;

        void Wrapper(Entity entity, BaseEntityEvent evt)
        {
            if (evt is TEvent typedEvent) handler(entity, typedEvent);
        }
    }
    
    public void Raise<TEvent>(TEvent ev) where TEvent : BaseEvent
    {
        var type = typeof(TEvent);
        if (_eventSub.TryGetValue(type, out var handlers))
            foreach (var handler in handlers.Cast<Action<TEvent>>())
                handler(ev);


        if (ev is not BaseEntityEvent entityEv)
            return;

        if (_entityEventSub.TryGetValue(type, out var entSubs))
            foreach (var callback in entSubs)
                ((Action<Entity, BaseEntityEvent>)callback)(entityEv.Ent, entityEv);     
        
        if (_entityEventCompSub.TryGetValue(type, out var entCompSubs)) 
            foreach (var (callback, compType) in entCompSubs)
                if (_comp.HasComponent(entityEv.Ent, compType))
                    ((Action<Entity, BaseEntityEvent>)callback)(entityEv.Ent, entityEv);
    }
}

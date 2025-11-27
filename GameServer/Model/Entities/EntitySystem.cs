using GameServer.Model.EventBus;
using GameServer.Model.Games;
using GameServer.Model.IoC;

namespace GameServer.Model.Entities;


public sealed class EntitySystem : BaseSystem
{
    [Dependency] private EventBusSystem _event = null!;
    
    private readonly EntityIdGenerator _generator = new();

    
    public Entity? GetEntity(ulong entityId, Game game)
    {
        if (game.Entities.TryGetValue(entityId, out var entity))
            return new Entity(entity, game);
        return null;
    }
    
    public Entity CreateEntity(Game game)
    {
        var entityId = _generator.Generate();
        
        var entity = new EntityInfo(entityId);
        game.Entities.Add(entityId, entity);
        return new Entity(entity, game);
    }

    public void DeleteEntity(Entity entity)
    {
        var ev = new EntityRemoveEvent(entity);
        _event.Raise(ev);
        
        entity.Game.Entities.Remove(entity.Info.Id);
    }

    private sealed class EntityIdGenerator()
    {
        private ulong _last = 0;
        public ulong Generate()
        {
            return _last++;
        }
    }
}
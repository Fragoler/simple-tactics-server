using GameServer.Model.Entities;

namespace GameServer.Model.EventBus;


public abstract class BaseEntityEvent : BaseEvent
{
    public Entity Ent;

    public BaseEntityEvent(Entity ent)
    {
        Ent = ent;
    }
}
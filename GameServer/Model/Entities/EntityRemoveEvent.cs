using GameServer.Model.EventBus;

namespace GameServer.Model.Entities;


public sealed class EntityRemoveEvent(Entity ent) : BaseEntityEvent(ent);
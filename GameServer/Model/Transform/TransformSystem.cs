using GameServer.Model.Components;
using GameServer.Model.Entities;
using GameServer.Model.IoC;


namespace GameServer.Model.Transform;


public sealed class TransformSystem : BaseSystem
{
    [Dependency] private ComponentSystem _component = null!;
    
    
    public void MoveEntity(Entity entity, Coordinates coords)
    {
        var xform = _component.EnsureComponent<TransformComponent>(entity);
        xform.Coords = coords;
    }
}
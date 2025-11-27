using GameServer.Model.Components;

namespace GameServer.Model.Transform;

public sealed class TransformComponent : Component
{
    public Coordinates Coords = new Coordinates(0, 0);
}

public struct Coordinates(uint x, uint y)
{
    public uint X = x;
    public uint Y = y;
}    

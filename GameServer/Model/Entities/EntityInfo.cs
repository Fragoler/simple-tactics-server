using GameServer.Model.Components;
using GameServer.Model.Games;

namespace GameServer.Model.Entities;


public class EntityInfo(ulong id)
{
    public readonly ulong Id = id;
    
    public readonly Dictionary<Type, Component> Components = [];
}

public record struct Entity(EntityInfo Info, Game Game)
{
    public readonly EntityInfo Info = Info;
    public readonly Game Game = Game;
}

// public record struct Entity<T>(Entity Ent, T Comp)
//     where T : Component
// {
//     public readonly Entity Ent = Ent;
//     public readonly T Comp = Comp;
// }
//
// public struct Entity<T1, T2>(Entity ent, T1 comp1, T2 comp2) 
//     where T1 : Component  
//     where T2 : Component
// {
//     public readonly Entity Ent = ent;
//     public readonly T1 Comp1 = comp1;
//     public readonly T2 Comp2 = comp2;
// }
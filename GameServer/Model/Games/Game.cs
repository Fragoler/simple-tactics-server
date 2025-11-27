using GameServer.Model.Components;
using GameServer.Model.Entities;
using GameServer.Model.Players;

namespace GameServer.Model.Games;


public class Game(string token)
{
    public readonly string Token = token;
    public List<(Entity, PlayerComponent)> Players = [];
    
    // Game entities
    public readonly Dictionary<ulong, Entities.EntityInfo> Entities = new();
    public readonly Dictionary<Type, HashSet<Component>> Components = new();
}
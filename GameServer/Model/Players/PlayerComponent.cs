using GameServer.Model.Components;
using GameServer.Model.Games;

namespace GameServer.Model.Players;


public sealed class PlayerComponent : Component
{
    public string PlayerToken = "";

    public Game Game = null!;
}
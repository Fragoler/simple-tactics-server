using System.Diagnostics.CodeAnalysis;
using GameServer.Model.IoC;
using GameServer.Model.Players;

namespace GameServer.Model.Games;


public sealed class GamesSystem : BaseSystem
{
    [Dependency] private PlayersSystem _players = null!;
    

    private readonly Dictionary<string, Game> _games = [];


    public string CreateGame()
    {
        var gameToken = Guid.NewGuid().ToString();
        var game = new Game(gameToken);
        _games.Add(gameToken, game);
        return game.Token;
    }
    
    public void DeleteGame(string token)
    {
        if (!TryGetGameByToken(token, out var game))
            return;

        foreach (var (ent, player) in game.Players)
            _players.DetachPlayerFromGame(player.PlayerToken);
        
        _games.Remove(token);
    }

    public Game? GetGame(string token)
    {
        return !TryGetGameByToken(token, out var game) ? null : game;
    }
    
    public IEnumerable<Game> GetGames()
    {
        return _games.Values;
    }
    
    public bool TryAddPlayer(string token, out string playerToken)
    {
        playerToken = "";
        
        if (!TryGetGameByToken(token, out var game))
            return false;
        
        _players.CreateAttachedPlayer(game, out var ent, out var player);
        game.Players.Add((ent, player));
        playerToken = player.PlayerToken;
        
        return true;
    }

    private bool TryGetGameByToken(string token, [NotNullWhen(true)] out Game? game)
    {
        return _games.TryGetValue(token, out game);
    }
}
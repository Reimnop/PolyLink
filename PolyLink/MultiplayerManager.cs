using System.Collections.Generic;
using PolyLink.Common.Data;

namespace PolyLink;

public class MultiplayerManager
{
    public int LocalPlayerId => localPlayerId;
    
    private int localPlayerId;
    private readonly Dictionary<int, PlayerInfo> players = new();
    
    public void SetData(int localPlayerId, ICollection<PlayerInfo> players)
    {
        this.localPlayerId = localPlayerId;
        
        this.players.Clear();
        foreach (var player in players)
            this.players[player.Id] = player;
    }
}
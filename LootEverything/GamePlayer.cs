using EFT;
using System;

namespace LootEverything
{
    public class GamePlayer
    {
        public Player Player { get; }

        public bool IsAI { get; private set; }

        public GamePlayer(Player player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            Player = player;
            IsAI = true;
        }
    }
}
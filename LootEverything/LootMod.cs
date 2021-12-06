using Comfort.Common;
using EFT;
using EFT.Interactive;
using MelonLoader;
using System.Collections.Generic;
using UnityEngine;

namespace LootEverything
{
    public class LootMod : MelonMod
    {
        public static Player LocalPlayer { get; set; }
        public static GameWorld GameWorld { get; set; }
        internal static List<LootItem> remainingItems = new List<LootItem>();
        internal static List<LootableContainer> remainingContainers = new List<LootableContainer>();
        internal static List<GamePlayer> Players = new List<GamePlayer>();
        private float recountItemsTime;
        private static readonly float itemsInterval = 1f;
        private static int _listCount = 0;
        private float _nextMainCacheTime;
        private float _cacheMainInterval = 5f;
        //private float _EasterEggInterval = 2f;
        //private float _EasterEggTime;
        //System.Random rnd = new System.Random();

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (Time.time >= _nextMainCacheTime)
            {
                UpdateMain();
                _nextMainCacheTime = Time.time + _cacheMainInterval;
            }
            if (Time.time >= recountItemsTime)
            {
                CountItems();
                GetPlayers();
                recountItemsTime = Time.time + itemsInterval;
            }
        }

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            MelonLogger.Msg("im currently in a JET melonloader mods folder");
        }

        public override void OnGUI()
        {
            base.OnGUI();
            //int random = rnd.Next(1, 1000);
            GUI.enabled = true;
            Vector2 nativeSize = new Vector2(1920, 1080);
            Vector3 scale = new Vector3(Screen.width / nativeSize.x, Screen.height / nativeSize.y, 1.0f);
            GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, scale);
            /*if (random == 235 && Time.time >= _EasterEggTime)
            {
                GUI.Label(new Rect(21, 71, 300f, 35f), "Fucking rip bro");
                _EasterEggTime = Time.time + _EasterEggInterval;

            }
            else if (Time.time >= _EasterEggTime) 
            {
                
            }*/
            GUI.Label(new Rect(21, 41, 300f, 35f), $"Loose Item Count: {remainingItems.Count}");
            GUI.Label(new Rect(21, 56, 300f, 35f), $"Containers with loot: {remainingContainers.Count}");
            GUI.Label(new Rect(21, 26, 300f, 35f), $"Bots currently alive: {Players.Count}");
            //GUI.Button(new Rect(21, 71, 35f, 35f), new GUIContent()
            //{
            //
            //});
            //MelonLogger.Msg($"Loose Item Count: {remainingItems.Count}");
            //MelonLogger.Msg($"Containers with loot: {remainingContainers.Count}");
        }

        private void GetPlayers()
        {
            try
            {
                if (Players.Count + 1 != Singleton<GameWorld>.Instance.RegisteredPlayers.Count)
                {
                    Players.Clear();
                    var enumerator = GameWorld.RegisteredPlayers.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        Player player = enumerator.Current;
                        if (player == null)
                            return;

                        if (player.IsYourPlayer())
                        {
                            LocalPlayer = player;
                            continue;
                        }

                        Players.Add(new GamePlayer(player));
                    }
                }
            }
            catch
            {
            }
        }

        private void CountItems()
        {
            try
            {
                if (_listCount != Singleton<GameWorld>.Instance.LootList.FindAll(item => item is LootableContainer || item is LootItem).Count)
                {
                    var enumerator = Singleton<GameWorld>.Instance.LootList.FindAll(item => item is LootableContainer || item is LootItem).GetEnumerator();
                    _listCount = Singleton<GameWorld>.Instance.LootList.FindAll(item => item is LootableContainer || item is LootItem).Count;

                    remainingItems.Clear();
                    remainingContainers.Clear();
                    while (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;

                        if (current is LootItem lootItem and not Corpse)
                        {
                            remainingItems.Add(lootItem);
                        }

                        if (current is LootableContainer lootableContainer && (!(current is Corpse)))
                        {
                            var rootContainer = lootableContainer.ItemOwner.RootItem;
                            int containerAmount = 0;

                            foreach (var item in rootContainer.GetAllItems())
                            {
                                containerAmount++;
                            }

                            //MelonLogger.Msg($"Container has {containerAmount} items inside.");

                            if (containerAmount >= 2)
                            {
                                remainingContainers.Add(lootableContainer);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void UpdateMain()
        {
            try
            {
                GameWorld = Singleton<GameWorld>.Instance;
            }
            catch
            {
            }
        }
    }
}
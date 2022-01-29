using Comfort.Common;
using EFT;
using EFT.Interactive;
using MelonLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LootEverything
{
    public class LootMod : MelonMod
    {

        #region Variable Declarations
        //public static MelonPreferences_Category LootEverythingSettings;
        //public static MelonPreferences_Entry<bool> _showBotCount;
        //public static MelonPreferences_Entry<bool> _showLooseItemCount;
        //public static MelonPreferences_Entry<bool> _showRemainingContainerCount;
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
        private bool _activeEgg = false;
        private bool inRaid = false;
        bool _showEasterEgg = false;
        System.Random rnd = new System.Random();
        #endregion

        #region Melonloader methods
        public override void OnUpdate()
        {
            base.OnUpdate();
            // Check if the current time is greater than the last cache time, and check if the player is in a raid
            if (Time.time >= _nextMainCacheTime && inRaid)
            {
                // Update the game world
                UpdateMain();
                // Set the next cache time
                _nextMainCacheTime = Time.time + _cacheMainInterval;
            }
            // Check if the current time is greater than the last item/bot count time as well as check if the player is in raid.
            if (Time.time >= recountItemsTime && inRaid)
            {
                // Update the counters
                CountItems();
                GetPlayers();
                // Set the next cache time
                recountItemsTime = Time.time + itemsInterval;
            }
            // Checks if the player is no longer in raid
            else if (!inRaid) 
            {
                // Clear the lists so they no longer take any more unnecessary resources
                Players.Clear();
                remainingContainers.Clear();
                remainingItems.Clear();
            }
            // Easter egg
            int random = rnd.Next(1, 10);
            if (random == 5 && !_activeEgg)
            {
                ToggleEasterEgg();
               
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) 
        {
            // Check the game scene's name to see if player is in raid
            switch (sceneName)
            {
                // In raid
                case "GameUIScene":
                    inRaid = true;
                    break;
                // Not in raid
                case "SessionEndUIScene":
                    inRaid = false;
                    break;
                default:
                    break;
            }
        }

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            // Load the preferences
            //LootEverythingSettings = MelonPreferences.CreateCategory("Mod settings");
            //_showBotCount = (MelonPreferences_Entry<bool>)LootEverythingSettings.CreateEntry("Show the bot count?", true);
            //_showLooseItemCount = (MelonPreferences_Entry<bool>)LootEverythingSettings.CreateEntry("Show the loose item count?", true);
            //_showRemainingContainerCount = (MelonPreferences_Entry<bool>)LootEverythingSettings.CreateEntry("Show the remaining containers that have items left?", true);
            //MelonPreferences.Save();
            //LootEverythingSettings.SaveToXml("LootEverything settings");
            MelonLogger.Msg("im currently in a JET melonloader mods folder");
            
        }

        public override void OnGUI()
        {
            base.OnGUI();
            // Enable the GUI
            GUI.enabled = true;
            // Sets the native screen size that the tool is built for
            Vector2 nativeSize = new Vector2(1920, 1080);
            // Sets the scale so the UI remains the same size throughout any other screen sizes
            Vector3 scale = new Vector3(Screen.width / nativeSize.x, Screen.height / nativeSize.y, 1.0f);
            // Apply the scale to the GUI
            GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, scale);
            // Easter egg
            if (_showEasterEgg)
            {
                GUI.Label(new Rect(21, 71, 300f, 35f), "Fucking rip bro");
                //_EasterEggTime = Time.time + _EasterEggInterval;

            }
            // Check if the player is in raid
            if (inRaid) 
            {
                drawGUI(new Rect(21, 45, 300f, 35f), $"Loose Item Count: {remainingItems.Count}");
                drawGUI(new Rect(21, 60, 300f, 35f), $"Containers with loot: {remainingContainers.Count}");
                drawGUI(new Rect(21, 30, 300f, 35f), $"Bots currently alive: {Players.Count}");

                // Render the counters visible to the player
                //if (_showLooseItemCount.Value) 
                //{
                    //GUI.Label(new Rect(21, 41, 300f, 35f), $"Loose Item Count: {remainingItems.Count}");
                    //DrawOutline(new Rect(21, 41, 300f, 35f), $"Loose Item Count: {remainingItems.Count}", 10);
                //}
                //if (_showRemainingContainerCount.Value) 
                //{
                    //GUI.Label(new Rect(21, 56, 300f, 35f), $"Containers with loot: {remainingContainers.Count}");
                    //DrawOutline(new Rect(21, 56, 300f, 35f), $"Containers with loot: {remainingContainers.Count}", 10);
                //}
                //if (_showBotCount.Value) 
                //{
                    //GUI.Label(new Rect(21, 26, 300f, 35f), $"Bots currently alive: {Players.Count}");
                    //DrawOutline(new Rect(21, 26, 300f, 35f), $"Bots currently alive: {Players.Count}", 10);
                //}
            }

            //GUI.Label(new Rect(21, 71, 300f, 35f), $"Current kill count: {LocalPlayer.Profile.Stats.SessionCounters.Counters.}")
            //MelonLogger.Msg($"Loose Item Count: {remainingItems.Count}");
            //MelonLogger.Msg($"Containers with loot: {remainingContainers.Count}");
        }

        #endregion

        #region Data collection methods
        private void GetPlayers()
        {
            try
            {
                // Check if the players count, plus 1 due to local player, doesn't equal the current amount of "players" in the current raid
                if (Players.Count + 1 != Singleton<GameWorld>.Instance.RegisteredPlayers.Count)
                {
                    // Clear the list
                    Players.Clear();
                    // Cycle through the list of current "players"
                    var enumerator = GameWorld.RegisteredPlayers.GetEnumerator();
                    // For each value in the list do...
                    while (enumerator.MoveNext())
                    {
                        // Declare the current player
                        Player player = enumerator.Current;
                        // If the player is null (non-existent) return (end the method)
                        if (player == null)
                            return;
                        // Check if the current player is the local player
                        if (player.IsYourPlayer())
                        {
                            // Declare the local player
                            LocalPlayer = player;
                            continue;
                        }
                        // Add the "player" to the list to be displayed on screen
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
                // Checks if the total amount of lootable containers and loot items is not equal to the previous count
                if (_listCount != Singleton<GameWorld>.Instance.LootList.FindAll(item => item is LootableContainer || item is LootItem).Count)
                {
                    // Cycle through the list of current lootable containers and loot items
                    var enumerator = Singleton<GameWorld>.Instance.LootList.FindAll(item => item is LootableContainer || item is LootItem).GetEnumerator();
                    // Declare the total amount of lootable containers and loot items
                    _listCount = Singleton<GameWorld>.Instance.LootList.FindAll(item => item is LootableContainer || item is LootItem).Count;
                    // Clear the lists of lootable containers and loot items
                    remainingItems.Clear();
                    remainingContainers.Clear();
                    // For each item in the LootList do....
                    while (enumerator.MoveNext())
                    {
                        // Ease of access
                        var current = enumerator.Current;
                        // Check if the current list entry is a loot item and not a corpse (not checking for a corpse will increase the number for each dead body)
                        if (current is LootItem lootItem and not Corpse)
                        {
                            // Add the loot item to the list of remaining items
                            remainingItems.Add(lootItem);
                        }
                        // Check if the current list entry is a lootable container and not a corpse (same bug as above)
                        if (current is LootableContainer lootableContainer && (!(current is Corpse)))
                        {
                            // Declare the containers inventory
                            var rootContainer = lootableContainer.ItemOwner.RootItem;
                            // Declare basic int value to count items
                            int containerAmount = 0;
                            // Count each item inside of the container
                            foreach (var item in rootContainer.GetAllItems())
                            {
                                // Increase the container items count by 1
                                containerAmount++;
                            }

                            //MelonLogger.Msg($"Container has {containerAmount} items inside.");
                            // Check if the container is empty (the container inventory itself counts as an item, so we check if the count is greater or equal to 2
                            if (containerAmount >= 2)
                            {
                                // Add the container to the remaining container list to be displayed to the local player
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
                // Updates the game world so we know everything in the current raid
                GameWorld = Singleton<GameWorld>.Instance;
            }
            catch
            {
            }
        }
        #endregion
        #region Helpers
        // Get out of here, it's a secret!
        IEnumerator ToggleEasterEgg() 
        {
            _showEasterEgg = true;
            _activeEgg = true;
            yield return new WaitForSeconds(2);
            _showEasterEgg = false;
            _activeEgg = false;
        }
        // Credits to the original creator of this code
        public void drawGUI(Rect position, string text)
        {
            GUIStyle style = new GUIStyle();
            int borderWidth = 2;
            //style.fontSize = 150; 
            //style.fontStyle = FontStyle.Bold;
            DrawTextWithOutline(position, text, style, Color.black, Color.white, borderWidth);

        }
        // Credits to the original creator of this code
        void DrawTextWithOutline(Rect centerRect, string text, GUIStyle style, Color borderColor, Color innerColor, int borderWidth)
        {
            // assign the border color
            style.normal.textColor = borderColor;
            // draw an outline color copy to the left and up from original
            Rect modRect = centerRect;
            modRect.x -= borderWidth;
            modRect.y -= borderWidth;
            GUI.Label(modRect, text, style);
            // stamp copies from the top left corner to the top right corner
            while (modRect.x <= centerRect.x + borderWidth)
            {
                modRect.x++;
                GUI.Label(modRect, text, style);
            }
            // stamp copies from the top right corner to the bottom right corner
            while (modRect.y <= centerRect.y + borderWidth)
            {
                modRect.y++;
                GUI.Label(modRect, text, style);
            }
            // stamp copies from the bottom right corner to the bottom left corner
            while (modRect.x >= centerRect.x - borderWidth)
            {
                modRect.x--;
                GUI.Label(modRect, text, style);
            }
            // stamp copies from the bottom left corner to the top left corner
            while (modRect.y >= centerRect.y - borderWidth)
            {
                modRect.y--;
                GUI.Label(modRect, text, style);
            }
            // draw the inner color version in the center
            style.normal.textColor = innerColor;
            GUI.Label(centerRect, text, style);
        }

        #endregion

    }
}
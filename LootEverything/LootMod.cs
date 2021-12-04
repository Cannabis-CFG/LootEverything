using Comfort.Common;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using MelonLoader;
using Harmony;

namespace LootEverything
{
    public class LootMod : MelonMod
    {
        internal static List<LootItem> remainingItems = new List<LootItem>();
        internal static List<LootableContainer> remainingContainers = new List<LootableContainer>();
        private float recountItemsTime;
        private static readonly float itemsInterval = 1f;
        private static int _listCount = 0;

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (Time.time >= recountItemsTime)
            {
                CountItems();

                recountItemsTime = Time.time + itemsInterval;
            }
        }

        public override void OnApplicationStart() 
        {
            base.OnApplicationStart();
            MelonLogger.Msg("Haha melonloader go brrrt");
        }


        public override void OnGUI()
        {
            base.OnGUI();
            GUI.enabled = true;
            Vector2 nativeSize = new Vector2(1920, 1080);
            Vector3 scale = new Vector3(Screen.width / nativeSize.x, Screen.height / nativeSize.y, 1.0f);
            GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, scale);
            GUI.Label(new Rect(21, 41, 300f, 35f), $"Loose Item Count: {remainingItems.Count}");
            GUI.Label(new Rect(21, 56, 300f, 35f), $"Containers with loot: {remainingContainers.Count}");
            //MelonLogger.Msg($"Loose Item Count: {remainingItems.Count}");
            //MelonLogger.Msg($"Containers with loot: {remainingContainers.Count}");
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

                        if (current is LootItem lootItem)
                        {
                            
                           remainingItems.Add(lootItem);
                            
                        }

                        if (current is LootableContainer lootableContainer)
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

    }
}

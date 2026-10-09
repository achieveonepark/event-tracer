using UnityEngine;

namespace Achieve.EventTracer.Samples
{
    public sealed class ShopTraceDemo : MonoBehaviour
    {
        public int gold = 500;
        public int price = 100;
        public bool duplicateCharge = true;
        private int items;

        private void Start() => ResetShop();

        public void ResetShop()
        {
            gold = 500;
            items = 0;
            EventTrace.StartSession("Shop demo", new { scenario = "purchase", duplicateCharge });
            EventTrace.Record("Shop.Ready", new { gold }, source: this);
        }

        public void Buy()
        {
            var purchase = EventTrace.Record("Shop.Purchase", new { itemId = "FireSword", price }, source: this);
            if (gold < price)
            {
                EventTrace.Record("Shop.Rejected", new { reason = "Insufficient gold" }, source: this,
                    parentId: purchase, level: TraceLevel.Warning);
                return;
            }
            Spend(purchase);
            if (duplicateCharge) Spend(purchase);
            items++;
            EventTrace.Record("Inventory.Add", new { itemId = "FireSword", count = items }, source: this, parentId: purchase);
        }

        private void Spend(TraceId parent)
        {
            var before = gold;
            gold -= price;
            EventTrace.Record("Wallet.Spend", new { before, after = gold, amount = price }, source: this, parentId: parent);
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(28, 28, 400, 300), GUI.skin.box);
            GUILayout.Label("EVENT TRACER / SHOP DEMO");
            GUILayout.Space(12);
            GUILayout.Label($"Gold: {gold}    Items: {items}    Price: {price}");
            duplicateCharge = GUILayout.Toggle(duplicateCharge, "Intentional duplicate charge");
            if (GUILayout.Button("Buy FireSword")) Buy();
            if (GUILayout.Button("Reset shop / new session")) ResetShop();
            GUILayout.Space(12);
            GUILayout.Label("Open Tools / Achieve / Event Tracer.\nSelect Shop.Purchase to follow both Wallet.Spend events.");
            GUILayout.EndArea();
        }
    }
}

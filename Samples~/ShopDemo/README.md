# Shop Demo

[한국어](README.ko.md)

Import **Shop Demo** from Package Manager, then run **Tools → Achieve → Samples → Create Shop Trace Demo**.

Enter Play mode and click **Buy FireSword** in the Game view. Open **Tools → Achieve → Event Tracer** and select `Shop.Purchase`: the two `Wallet.Spend` events and `Inventory.Add` are linked as effects of the same purchase.

Turn off **Intentional duplicate charge** and purchase again to see a single charge. **Reset shop / new session** resets gold and starts a new trace session.

To inspect the synchronous call chain, enable **Capture limits → Capture stack trace → Apply** before purchasing, then select a new event and read **CALL STACK**.

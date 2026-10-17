# Shop Demo

[English](README.md)

Package Manager에서 **Shop Demo**를 Import하고 **Tools → Achieve → Samples → Create Shop Trace Demo**를 실행합니다.

Play 후 Game 뷰에서 **Buy FireSword**를 누릅니다. **Tools → Achieve → Event Tracer**에서 `Shop.Purchase`를 선택하면 두 번 발생한 `Wallet.Spend`와 `Inventory.Add`가 같은 구매의 결과로 연결됩니다.

Game 뷰의 **Intentional duplicate charge**를 끄고 다시 구매하면 차감이 한 번만 발생합니다. **Reset shop / new session**은 골드를 초기화하고 새 추적 세션을 시작합니다.

호출 체인을 확인하려면 구매 전에 **Capture limits → Capture stack trace → Apply**를 켜고 새 이벤트의 **CALL STACK**을 확인하세요.

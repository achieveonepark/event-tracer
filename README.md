# Event Tracer

게임 이벤트의 데이터와 원인·결과 관계를 기록하고 `.etr` 캡처로 저장하는 Unity 패키지입니다.

## 설치

Unity **2022.3 이상**에서 Package Manager → **+ → Add package from git URL**:

```text
https://github.com/achieveonepark/event-tracer.git
```

## 사용

**Tools → Achieve → Event Tracer**를 열고 게임 코드에 기록 지점을 추가합니다.

```csharp
using Achieve.EventTracer;

EventTrace.StartSession("Shop test");
var purchase = EventTrace.Record("Shop.Purchase", new { price = 100 });
EventTrace.Record("Wallet.Spend", new { amount = 100 }, parentId: purchase);
```

Unity 메인 스레드에서 호출하며, Editor/Development Build에서 수집합니다. 창에서 검색·필터·원인 추적·캡처 저장을 사용할 수 있고 **한국어 / English**를 선택할 수 있습니다.

샘플: Package Manager의 **Shop Demo**를 Import → **Tools → Achieve → Samples → Create Shop Trace Demo**.

자세한 API와 캡처 형식은 [사용 가이드](Documentation~/event-tracer.md)를 참고하세요.

## AI 스킬

저장소에 Codex용 [`achieve-event-tracer`](.agents/skills/achieve-event-tracer/SKILL.md)와 Claude Code용 [동일 스킬](.claude/skills/achieve-event-tracer/SKILL.md)이 포함되어 있습니다. 직접 호출: Codex `$achieve-event-tracer`, Claude Code `/achieve-event-tracer`.

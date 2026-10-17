# Event Tracer

[English](README.md)

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

자세한 API와 캡처 형식은 [사용 가이드](Documentation~/event-tracer.ko.md)를 참고하세요.

## 호출 스택

**Capture limits → Capture stack trace → Apply**를 켜면 이후 이벤트를 기록하는 시점의 호출 스택을 함께 저장합니다. 이벤트를 선택하면 **CALL STACK**에서 스택을 조회·복사할 수 있으며 `.etr` 내보내기에도 포함됩니다.

```csharp
var options = EventTrace.Configuration;
options.captureStackTrace = true;
options.maxStackTraceCharacters = 8192;
EventTrace.Configure(options);
```

호출 스택 수집은 기록 비용이 있어 기본적으로 꺼져 있습니다. 스택별 문자 제한과 전체 버퍼 예산이 적용됩니다. 파일·줄 번호는 디버그 심볼에 따라 달라집니다. 비동기·코루틴 경계를 넘는 인과 관계에는 `parentId`를 사용하세요.

## 배포와 라이선스

버전은 **1.0.0**, 라이선스는 [MIT](LICENSE.md)입니다. 패키지 ID는 `com.achive.event-tracer`입니다. [OpenUPM 배포 안내](Documentation~/publishing.ko.md)를 참고하세요.

## AI 스킬

스킬 원본은 [`Skills~/achieve-event-tracer`](Skills~/achieve-event-tracer/SKILL.md)에 있습니다. Unity Skill Manager가 프로젝트의 `.agents/skills`와 `.claude/skills`에 취합합니다. 직접 호출: Codex `$achieve-event-tracer`, Claude Code `/achieve-event-tracer`.

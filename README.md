# Event Tracer

게임에서 발생하는 이벤트의 데이터와 원인 관계를 기록하는 독립 Unity 패키지입니다.

- 패키지: `com.achive.event-tracer`
- 런타임 네임스페이스/어셈블리: `Achieve.EventTracer`
- 최소 Unity: 2022.3. 현재 검증 에디터: 6000.5.10f1.

## 빠른 시작

Package Manager에서 이 폴더의 `package.json`을 설치하고 **Tools → Achieve → Event Tracer**를 엽니다. 게임 코드에서 기록할 지점에 호출을 추가합니다.

```csharp
using Achieve.EventTracer;

EventTrace.StartSession("Boss test", new { seed = 1234 });
var purchase = EventTrace.Record("Shop.Purchase", new { itemId = "sword", price = 100 });
EventTrace.Record("Wallet.Spend", new { before = 500, after = 400 },
    parentId: purchase, source: this);
```

`Record`는 세션 ID와 순번을 가진 `TraceId`를 반환합니다. 비동기/코루틴 작업에도 동일한 ID를 전달해야 원인과 결과가 연결됩니다. 발생 시간이 가깝다는 이유로 관계를 추측하지 않습니다.

## 에디터 창

- 이름·데이터 검색, 카테고리·심각도·소스 이름 필터.
- 시간·프레임·원인 관계 들여쓰기가 있는 가상화 목록.
- 원인/결과 바로가기, 페이로드 스냅샷, 살아 있는 소스 오브젝트 선택, 호출 코드 열기.
- **Record**: 기록 활성화/비활성화.
- **Pause view**: 화면의 캡처만 고정하며 기록은 계속됩니다.
- **New session**: 세션을 시작하고 버퍼를 비웁니다.
- **Clear**: 현재 버퍼를 비우고 같은 세션의 순번을 유지합니다.
- **Export / Import**: `.etr` JSON 캡처 저장과 조회. 기존 `.json` 캡처도 읽을 수 있습니다. Import는 라이브 기록을 변경하지 않습니다.
- **Capture limits**: 이벤트 개수, 페이로드 문자 수, 전체 기록 문자열 예산, 호출 위치 기록 설정.

`Assets` 안에 `.etr`을 저장하면 Unity가 캡처 파일로 인식합니다. Project 창에서 선택하면 UI Toolkit 인스펙터에 세션 정보와 이벤트 미리보기가 나오며, **Open in Event Tracer**로 전체 캡처를 조회할 수 있습니다. 캡처 인스펙터는 기록을 읽기 전용으로 표시합니다.

창과 파일 인스펙터의 **한국어 / English** 메뉴로 언어를 선택합니다. Play Bookmarks와 같은 설정을 사용합니다. 이벤트 이름·소스 이름·페이로드·검색어는 바뀌지 않습니다.

## 기록 범위

게임 이벤트를 기록할 위치는 직접 지정하거나 기존 이벤트 버스의 발행/수신 어댑터에 연결합니다. 자동으로 모든 C# 이벤트를 가로채는 기능은 포함하지 않습니다.

기록은 **Unity 메인 스레드**에서 호출합니다. 워커 스레드에서 발생한 데이터는 게임의 메인 스레드 큐로 전달하세요. Editor/Development Build에서만 수집하며 일반 릴리스 빌드는 유효하지 않은 `TraceId`를 반환합니다. 인자는 호출 전에 평가되므로 비싼 DTO 생성은 `if (EventTrace.IsEnabled)`로 감싸세요.

공개 필드와 읽을 수 있는 공개 프로퍼티를 스냅샷으로 복사합니다. 디버그용 DTO/익명 객체를 전달하세요. 깊이 4, 컬렉션/객체 항목 32개와 문자 수 제한을 적용합니다. Unity 오브젝트는 이름·ID·타입만 저장합니다. 데이터가 너무 크면 유효한 JSON인 `{"$truncated":true}`를 저장합니다.

## 샘플

Package Manager에서 **Shop Demo**를 Import하고 **Tools → Achieve → Samples → Create Shop Trace Demo**를 실행합니다. 구매 시 의도적인 중복 골드 차감이 발생합니다. 추적 창에서 구매 이벤트를 선택하면 두 차감이 같은 원인에 연결된 것을 확인할 수 있습니다.

[런타임 API와 저장 형식](Documentation~/event-tracer.md)

## Codex 스킬

이 저장소의 `.agents/skills/achieve-event-tracer`에 구현·연동 스킬이 포함되어 있습니다. 관련 작업에서 자동으로 선택하도록 설정되어 있으며, `$achieve-event-tracer`로 직접 호출할 수도 있습니다. `AGENTS.md`도 패키지 작업을 이 스킬로 연결합니다.

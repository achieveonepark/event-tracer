# Event Tracer 구현 가이드

## 연동 예제

```csharp
using Achieve.EventTracer;

EventTrace.StartSession("Boss test", new { seed = 1234 });
var purchase = EventTrace.Record("Shop.Purchase", new { itemId = "sword", price = 100 });
EventTrace.Record("Wallet.Spend", new { before = 500, after = 400 }, parentId: purchase);
```

MonoBehaviour에서는 `source: this`로 소스를 지정할 수 있다. 비동기 작업도 반환받은 `TraceId`를 전달한다. caller file/member/line은 caller attributes가 채우므로 일반 호출에서 수동으로 지정하지 않는다.

| API | 동작 |
| --- | --- |
| `Configure(TraceConfiguration)` | 검증된 설정 복사, 크기 변경 시 최신 기록 보존 |
| `StartSession(name, metadata)` | 새 세션 ID와 메타데이터 스냅샷, 버퍼 초기화 |
| `Record(...)` | 읽기 전용 레코드, 복사된 페이로드와 명시적 부모 ID |
| `Clear()` | 같은 세션에서 버퍼만 초기화, 순번 재사용 안 함 |
| `GetSnapshot()` / `Capture()` | 현재 레코드 배열 / 독립된 세션 캡처 |
| `TraceCapture.ToJson()` / `FromJson(json)` | formatVersion 1 저장 / 검증된 독립 캡처 읽기 |

## 내부 소스

| 작업 | 관련 소스 |
| --- | --- |
| 수집·세션·빌드 정책 | `Runtime/Core/EventTrace.cs` |
| 순환 버퍼 | `Runtime/Collections/TraceBuffer.cs` |
| 레코드·부모·설정·캡처 | `Runtime/Models/` |
| 스냅샷·오브젝트 ID | `Runtime/Serialization/PayloadSnapshot.cs`, `TraceObjectIdentity.cs` |
| 캡처 파일 | `Editor/Importers/TraceCaptureImporter.cs`, `Editor/Inspectors/TraceCaptureImporterEditor.cs` |
| 필터·라이브·원인 조회 | `Editor/Windows/EventTraceWindow.cs` |
| 언어·설정·스타일 | `Editor/Services/`, `Editor/UI/Styles/EventTracer.uss` |

기본 제한은 이벤트 10,000개, 페이로드 4,096자, 전체 레코드 문자열 4M자다. 문자 예산은 총 프로세스 메모리의 정확한 한도가 아니다. 오래된 기록을 버리면 부모 레코드가 사라질 수 있으므로 이를 표시하고 다른 세션의 같은 순번에 연결하지 않는다.

스냅샷은 공개 필드·프로퍼티를 읽으며 기본 깊이 4, 항목 32개 제한을 둔다. 순환, 읽을 수 없는 getter와 초과 크기는 표시 문자열 또는 유효한 JSON truncation marker로 표현한다. getter의 재귀 Record는 무시하며 getter 비용 자체를 제한한다고 가정하지 않는다. Unity 오브젝트는 이름·ID·타입만 기록하고 살아 있는 소스는 weak reference로 보유한다.

## 파일과 UI

`.etr`은 세션, 레코드, 폐기 개수를 저장한다. 현재 읽기 한도는 16M문자와 100,000개 레코드이며 파일 읽기 전 16MB 크기도 확인한다. `TraceCaptureAsset`은 Importer가 만드는 캡처 조회용 오브젝트다. Importer 인스펙터는 원본 파일을 수정하지 않는다.

Pause view는 화면만 고정하며 수집은 계속한다. Import한 파일의 조회, 라이브 전환, 수집 활성화는 서로 구분한다. 필터의 실제 값과 표시 언어를 분리하고 `DropdownField`의 표시 callback을 사용한다. 번역이 실제 이벤트나 필터의 ID를 바꾸지 않도록 한다.

자주 갱신되는 요약·상태·버튼에는 `TraceLocalization.SetText(element, english)`를 사용한다. 값이 갱신될 때마다 영어로 잠시 돌아오지 않게 하고, 원문을 보관해 영어로 전환할 수 있도록 한다.

## 다른 패키지와 연동

북마크 실행 시 게임 측 구독자가 `RunStarted`에서 새 추적 세션을 만들고 `RunCompleted`에 결과를 기록할 수 있다. domain reload를 꺼도 구독이 중복되지 않도록 등록 전에 기존 핸들러를 제거한다.

Masked Values의 `.Value`를 페이로드로 전달하면 명시적으로 복원한 평문 스냅샷이 기록된다. 래퍼만 전달해 내부 저장 구조를 자동으로 분석하지 않는다. 예제는 알려진 원본 루트의 `Samples/AchieveIntegration/Runtime/IntegrationTraceBridge.cs`, `IntegrationDemoPlayer.cs`를 읽는다.

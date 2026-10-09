# Event Tracer API

## 에디터 창

**Tools → Achieve → Event Tracer**에서 기록을 조회합니다.

- 이름·데이터 검색, 카테고리·심각도·소스 이름 필터.
- 시간·프레임·원인 관계 들여쓰기가 있는 가상화 목록.
- 원인/결과 바로가기, 페이로드 스냅샷, 살아 있는 소스 선택, 호출 코드 열기.
- **Record**: 수집 활성화/비활성화. **Pause view**: 수집을 계속하면서 화면만 고정.
- **New session**: 새 세션 시작과 버퍼 초기화. **Clear**: 같은 세션과 순번을 유지하고 버퍼만 비움.
- **Export / Import**: `.etr` JSON 캡처 저장·조회. 기존 `.json` 캡처도 읽을 수 있으며 Import는 라이브 수집을 변경하지 않음.
- **Capture limits**: 이벤트 개수·페이로드 문자 수·전체 문자열 예산·호출 위치 설정.

`Assets` 안의 `.etr`을 선택하면 읽기 전용 UI Toolkit 인스펙터가 표시됩니다. **Open in Event Tracer**로 전체 캡처를 조회합니다. **한국어 / English** 선택은 Play Bookmarks와 공유하며 이벤트 이름·소스 이름·페이로드·검색어는 변경하지 않습니다.

Package Manager에서 **Shop Demo**를 Import하고 **Tools → Achieve → Samples → Create Shop Trace Demo**를 실행합니다. 의도적인 중복 골드 차감을 같은 구매 이벤트의 두 결과로 확인할 수 있습니다.

## 기록

```csharp
TraceId Record(string eventName, object data = null, UnityEngine.Object source = null,
    TraceId parentId = default, string category = null, TraceLevel level = TraceLevel.Info,
    string callerFile = "", string callerMember = "", int callerLine = 0);
```

호출 위치는 C# caller attributes로 자동 전달됩니다. 기본 카테고리는 이벤트 이름의 첫 `.` 앞 부분이며, 구분자가 없으면 `General`입니다. 전체 stack trace를 수집하지 않습니다.

`TraceRecord`는 읽기 전용 속성을 노출합니다. `GetSnapshot()`은 현재 버퍼의 순서대로 배열을 반환합니다. 소스 오브젝트는 weak reference로만 보유하며 삭제된 소스와 Import한 캡처에서는 선택 버튼이 제공되지 않습니다. 런타임 오브젝트 ID는 세션 간 복원 키로 사용하지 않습니다.

| API | 역할 |
| --- | --- |
| `EventTrace.Configure(options)` | 설정 복사·검증·버퍼 크기 변경. 최신 기록 유지 |
| `EventTrace.StartSession(name, metadata)` | 새 세션 ID 생성, 메타데이터 스냅샷, 버퍼 초기화 |
| `EventTrace.Record(...)` | 페이로드 스냅샷·호출 위치·명시적 부모 ID 저장 |
| `EventTrace.GetSnapshot()` | 현재 이벤트의 읽기 전용 레코드 배열 |
| `EventTrace.Capture()` | 세션·이벤트·버려진 이벤트 수의 분리된 캡처 |
| `EventTrace.Clear()` | 버퍼 초기화. 세션 ID/순번 유지 |
| `TraceCapture.ToJson()` | formatVersion 1 JSON 내보내기 |
| `TraceCapture.FromJson(json)` | 형식·ID·순번·부모 관계 검증 후 독립 캡처 반환 |

모든 기록/설정/캡처 변경 API는 메인 스레드용입니다. 에디터 설정은 EditorPrefs에 저장하며 Play 모드 진입 후 다시 적용합니다.

수집은 Editor/Development Build에서 동작하며 일반 릴리스 빌드는 유효하지 않은 `TraceId`를 반환합니다. 비싼 DTO 생성은 `if (EventTrace.IsEnabled)`로 감싸세요. 인자는 호출 전에 평가됩니다. 워커 스레드의 데이터는 게임의 메인 스레드 큐로 전달합니다.

기록 지점은 게임 코드나 이벤트 버스 어댑터에 직접 추가합니다. 모든 C# 이벤트를 자동으로 가로채지는 않습니다. `Record`가 반환한 세션 ID와 순번을 가진 `TraceId`를 비동기·코루틴 후속 작업에도 `parentId`로 전달하세요. 발생 시간만으로 인과 관계를 추측하지 않습니다.

## 용량과 스냅샷

기본 설정은 최대 10,000개 이벤트, 페이로드 4,096자, 레코드 문자열 총 4M자입니다. 개수 또는 문자열 예산을 초과하면 가장 오래된 기록을 버립니다. `DroppedCount`에 개수와 예산으로 버려진 기록을 포함합니다.

문자 예산은 관리 문자열 길이에 대한 제한이며 전체 프로세스의 정확한 메모리 사용량은 아닙니다. 레코드 객체, 배열, 렌더링, JSON 내보내기에도 추가 메모리가 필요합니다.

공개 필드와 읽을 수 있는 공개 프로퍼티를 스냅샷으로 복사합니다. 기본 깊이 4, 컬렉션·객체 항목 32개와 문자 수 제한을 적용합니다. Unity 오브젝트는 이름·ID·타입만 저장하며 초과 페이로드는 유효한 JSON인 `{"$truncated":true}`로 표시합니다.

문자열은 JSON escape, 숫자는 invariant culture, enum은 이름, DateTime은 ISO 형식으로 저장합니다. 순환은 `<cycle>`, 깊이 초과는 `<max-depth>`, 읽을 수 없는 프로퍼티는 `<unreadable: ...>`로 표현합니다. 프로퍼티 getter에서 재귀적으로 호출되는 Record는 무시합니다. getter의 실행 비용은 수집기가 제어할 수 없으므로 작은 데이터 객체를 사용하세요.

부모가 버퍼에서 사라지거나 이전 세션에 속하면 UI에 이를 표시하며, 현재 세션의 같은 순번으로 잘못 연결하지 않습니다.

## 저장

```csharp
var capture = EventTrace.Capture();
var json = capture.ToJson();
var restored = TraceCapture.FromJson(json);
```

JSON에는 세션 ID/이름/시작 UTC/메타데이터, 이벤트 ID/부모 ID, 시간/프레임, 카테고리/심각도, 페이로드, 소스 이름/오브젝트 ID, 호출 위치가 포함됩니다. Unity 오브젝트 참조는 내보내지 않습니다.

에디터의 기본 확장자는 `.etr`입니다. `Assets`에 있는 파일은 `TraceCaptureImporter`가 `TraceCaptureAsset`으로 임포트합니다. 인스펙터는 캡처 요약과 이벤트 미리보기를 표시하고 전체 캡처를 Event Tracer 창으로 엽니다. 런타임 API의 `ToJson`/`FromJson`은 파일 확장자에 의존하지 않습니다.

Importer는 최대 16M문자와 100,000개 기록을 허용합니다. UI에서 파일을 읽기 전에 16MB 파일 크기도 확인합니다. 스냅샷이 큰 세션은 내보내기 결과가 이 제한을 넘을 수 있으므로 수집 제한을 작게 설정하거나 캡처를 나누세요.

## 테스트

소비 프로젝트에서 테스트 프레임워크를 설치하고 `Packages/manifest.json`에 `"testables": ["com.achive.event-tracer"]`를 추가합니다. 런타임 패키지는 테스트 프레임워크나 Play Bookmarks를 의존하지 않습니다.

# Event Tracer API

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

## 용량과 스냅샷

기본 설정은 최대 10,000개 이벤트, 페이로드 4,096자, 레코드 문자열 총 4M자입니다. 개수 또는 문자열 예산을 초과하면 가장 오래된 기록을 버립니다. `DroppedCount`에 개수와 예산으로 버려진 기록을 포함합니다.

문자 예산은 관리 문자열 길이에 대한 제한이며 전체 프로세스의 정확한 메모리 사용량은 아닙니다. 레코드 객체, 배열, 렌더링, JSON 내보내기에도 추가 메모리가 필요합니다.

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

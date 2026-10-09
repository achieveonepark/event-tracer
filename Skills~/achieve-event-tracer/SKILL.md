---
name: achieve-event-tracer
description: Implement, extend, integrate, or debug Achieve Event Tracer (com.achive.event-tracer) in Unity. Use for 게임 이벤트 추적, EventTrace instrumentation, gameplay payload snapshots, explicit cause/effect parent IDs, .etr captures, and the UI Toolkit trace browser, even when this skill is not explicitly named. Excludes unrelated distributed tracing and automatic interception of every C# event.
---

# Achieve Event Tracer

Unity 게임의 이벤트 기록과 원인·결과 분석, 또는 이 패키지의 구현 변경에 사용한다.

## 패키지 찾기

- 패키지 ID는 `com.achive.event-tracer`, 네임스페이스와 런타임 어셈블리는 `Achieve.EventTracer`다. `achive` 철자를 유지한다.
- 현재 프로젝트에 설치된 패키지와 게임 asmdef를 확인한다. 알려진 원본은 `C:/Users/parka/orca/projects/ts1/unity-event-tracer`다. 실제 설치 버전의 API를 읽고 연동한다.
- 수집기·파일 형식·에디터 동작을 수정할 때 [구현 가이드](references/package-guide.md)를 읽는다. 패키지의 `README.md`, `Documentation~/event-tracer.md`에 자세한 API가 있다.

## 구현 기준

- 기록할 게임 액션이나 이벤트 버스 어댑터에 `EventTrace.Record`를 명시적으로 넣는다. Unity 메인 스레드에서 호출한다. 워커 스레드의 데이터는 게임의 메인 스레드 큐를 통해 전달한다.
- 반환된 `TraceId`를 후속 작업에 `parentId`로 전달한다. 세션 ID와 순번을 함께 보존하고, 시간이나 같은 순번만으로 인과 관계를 추측하지 않는다.
- 작은 DTO나 익명 객체를 스냅샷으로 전달한다. 값비싼 데이터 생성은 `EventTrace.IsEnabled`로 감싼다. 기본 빌드 정책은 Editor/Development Build에서만 수집하는 것이다.
- 개수와 문자열 예산을 모두 제한하는 ring buffer, 폐기 개수와 이전 세션 부모 표시를 유지한다. Import한 캡처 조회가 라이브 기록을 교체하지 않도록 한다.
- 기본 캡처 확장자는 `.etr`이다. `TraceCapture` JSON과 Importer, 읽기 전용 캡처 인스펙터를 유지한다. 기존 JSON 캡처도 읽을 수 있다.
- 에디터는 UI Toolkit, Unity의 회색 톤과 가상화된 목록을 사용한다. `TraceLocalization`의 한국어/English 선택과 `Achieve.Editor.Language` 설정을 유지한다. 갱신되는 문구는 `SetText`로 표시하고 이벤트 이름·소스 이름·JSON·검색값은 번역하지 않는다.
- Play Bookmarks 또는 Masked Values를 함께 쓸 때 해당 스킬도 적용하고 연동은 소비 게임 코드에 둔다. 이 런타임 패키지에 다른 Achieve 패키지의 필수 의존성을 추가하지 않는다.

## 확인

영향받는 동작에 대해 부모 ID, 기록 당시 데이터 복사, 버퍼 폐기, 세션 유지, JSON 왕복 저장을 확인한다. UI 변경은 실제 필터·선택과 한영 전환을 확인한다. 관련 테스트는 `Tests/Editor`에 있다.

알려진 세 패키지 연동 샘플은 `C:/Users/parka/orca/projects/ts1/Samples/AchieveIntegration`이다. 기존 이벤트 체계와 사용자의 이벤트 명명 규칙에 맞춰 적용한다.

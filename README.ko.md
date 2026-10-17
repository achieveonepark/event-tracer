# Event Tracer

[![openupm](https://img.shields.io/npm/v/com.achive.event-tracer?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.achive.event-tracer/)
[![openupm](https://img.shields.io/badge/dynamic/json?color=brightgreen&label=downloads&query=%24.downloads&suffix=%2Fmonth&url=https%3A%2F%2Fpackage.openupm.com%2Fdownloads%2Fpoint%2Flast-month%2Fcom.achive.event-tracer)](https://openupm.com/packages/com.achive.event-tracer/)

[English](README.md)

게임에서 무슨 일이 일어났고 왜 발생했는지 확인하는 Unity 도구입니다. 이벤트와 데이터를 기록하고, 관련 이벤트와 호출 스택을 살펴보고, 세션을 저장해 나중에 다시 확인하거나 공유할 수 있습니다.

## 설치

**Unity 2022.3 이상**이 필요합니다. 이벤트 기록은 Editor와 Development Build에서 동작합니다.

### OpenUPM

[OpenUPM CLI](https://openupm.com/docs/getting-started-cli.html)가 설치되어 있다면 Unity 프로젝트 폴더에서 실행합니다.

```sh
openupm add com.achive.event-tracer
```

Unity 에디터에서 직접 설치하려면:

1. **Edit → Project Settings → Package Manager**에서 Scoped Registry를 추가합니다.

   | 항목 | 값 |
   | --- | --- |
   | Name | `OpenUPM` |
   | URL | `https://package.openupm.com` |
   | Scope | `com.achive.event-tracer` |

2. **Apply**를 누르고 **Window → Package Manager**를 엽니다.
3. **+ → Add package by name**을 선택하고 `com.achive.event-tracer`를 입력한 뒤 **Add**를 누릅니다.

자세한 설치 과정은 [OpenUPM 설치 가이드](https://openupm.com/docs/getting-started.html)를 참고하세요.

### Git URL

Package Manager에서 **+ → Add package from git URL**을 선택하고 아래 주소로 설치할 수도 있습니다.

```text
https://github.com/achieveonepark/event-tracer.git
```

## 빠른 시작

**Tools → Achieve → Event Tracer**를 엽니다. 확인하려는 시나리오가 시작될 때 세션을 만들고, 관심 있는 동작을 기록합니다.

```csharp
using Achieve.EventTracer;

EventTrace.StartSession("Shop test");
var purchase = EventTrace.Record("Shop.Purchase", new { price = 100 });
EventTrace.Record("Wallet.Spend", new { amount = 100 }, parentId: purchase);
```

위 메서드는 Unity 메인 스레드에서 호출합니다. 앞서 기록한 이벤트 ID를 `parentId`로 전달하면 다음 이벤트의 원인으로 연결됩니다. `MonoBehaviour`에서 기록할 때 `source: this`를 추가하면 창에서 해당 오브젝트를 선택할 수 있습니다.

Play 모드에 들어가 창에서 이벤트를 선택하세요. 기록된 데이터, 원인과 결과, 소스 오브젝트와 호출 코드를 확인할 수 있습니다. 검색과 카테고리·심각도·소스 필터로 원하는 이벤트를 찾으세요.

UI 기본 언어는 영어입니다. 언어 메뉴에서 **한국어**로 바꿀 수 있으며 선택한 언어는 기억합니다.

## 데모 체험

1. Package Manager에서 **Event Tracer**를 선택하고 **Samples**의 **Shop Demo**를 Import합니다.
2. **Tools → Achieve → Samples → Create Shop Trace Demo**를 실행합니다.
3. Play 모드에 들어가 Game 뷰에서 **Buy FireSword**를 누릅니다.
4. Event Tracer에서 `Shop.Purchase`를 선택합니다. 이 구매의 결과로 `Wallet.Spend`가 두 개 표시됩니다. 데모에 의도적으로 넣은 중복 차감입니다.
5. **Intentional duplicate charge**를 끄고 다시 구매하면 한 번만 차감되는 흐름과 비교할 수 있습니다.

Game 뷰의 **Reset shop / new session**으로 처음부터 다시 시작할 수 있습니다.

## 창의 주요 버튼

| 버튼 | 용도 |
| --- | --- |
| **Record** | 이벤트 기록 켜기·끄기 |
| **Pause view** | 기록을 계속하면서 표시 화면만 고정 |
| **New session** | 새 세션을 시작하고 이벤트 목록 비우기 |
| **Clear** | 같은 세션을 유지하면서 목록만 비우기 |
| **Back to live** | 불러온 캡처나 고정 화면에서 현재 세션으로 돌아가기 |
| **Capture limits → Apply** | 기록 용량과 호출 위치·스택 수집 설정 |

## 호출 스택

문제를 재현하기 전에 **Capture limits → Capture stack trace**를 켜고 **Apply**를 누르세요. 이후 기록된 이벤트를 선택하면 **CALL STACK**에서 호출 스택을 확인하거나 복사할 수 있습니다.

호출 스택 기록은 비용이 있어 기본적으로 꺼져 있습니다. 파일·줄 번호는 디버그 심볼에 따라 달라집니다. 비동기 작업이나 코루틴으로 나뉜 이벤트는 `parentId`를 전달해 원인·결과 관계를 연결하세요.

## 세션 저장과 다시 열기

**Export**로 현재 표시 중인 세션을 `.etr` 파일로 저장합니다. 이벤트 데이터, 연결 관계와 기록된 호출 스택이 함께 저장됩니다. 보관할 기록은 새 세션을 시작하기 전에 내보내세요.

**Import**로 캡처를 열어도 라이브 기록은 바뀌지 않습니다. Project 창에서 `.etr` 파일을 선택하고 **Open in Event Tracer**를 눌러도 됩니다. **Back to live**로 현재 세션으로 돌아갈 수 있습니다.

## 문제 해결

- **이벤트가 안 보이나요?** **Record**를 확인하고 검색·필터를 지우세요. 게임에서 `EventTrace.Record`를 호출하는지 확인하세요. 기록은 Editor나 Development Build에서 동작합니다.
- **호출 스택이 없나요?** 스택 수집을 켜고 **Apply**를 누른 뒤 동작을 다시 재현하세요. 기존 이벤트에는 기록 당시 정보가 유지됩니다.
- **이전 이벤트나 부모가 사라졌나요?** 기록 한도에 도달하면 오래된 이벤트가 제거됩니다. **Capture limits**에서 한도를 늘리거나 짧은 세션으로 나누어 내보내세요.
- **소스 오브젝트를 선택할 수 없나요?** 기록할 때 `source`를 지정하세요. 오브젝트가 살아 있을 때 선택할 수 있으며, 저장된 캡처에는 실제 참조 대신 이름이 남습니다.

설정 예제와 캡처 한도는 [사용 및 API 가이드](Documentation~/event-tracer.ko.md)를 참고하세요.

## 라이선스

[MIT](LICENSE.md).

# OpenUPM 배포

[English](publishing.md) · [README](../README.ko.md)

패키지는 **1.0.0**, **MIT** 라이선스로 준비되어 있습니다. `package.json`은 저장소 루트에 있으며, 패키지 ID는 기존 철자인 `com.achive.event-tracer`를 유지합니다.

## 첫 배포

1. Unity EditMode 테스트를 실행하고 변경 사항을 검토한 뒤 `main`에 릴리스 커밋을 만듭니다.
2. 커밋과 버전 태그를 푸시합니다.

   ```sh
   git push origin main
   git tag v1.0.0
   git push origin v1.0.0
   ```

3. [OpenUPM 등록 양식](https://openupm.com/packages/add/)에 다음 값을 입력합니다.

   | 항목 | 값 |
   | --- | --- |
   | Package name | `com.achive.event-tracer` |
   | Display name | `Event Tracer` |
   | Repository | `https://github.com/achieveonepark/event-tracer` |
   | License | `MIT` |
   | Topic | `utilities` |
   | Minimum version | `1.0.0` |
   | README | `main:README.md` |
   | Hunter | `achieveonepark` |

4. 생성된 메타데이터를 OpenUPM 저장소에 제출하고 병합과 빌드를 기다립니다. 메타데이터 위치는 이 패키지 저장소가 아닌 OpenUPM의 `data/packages/com.achive.event-tracer.yml`입니다. 등록 후 패키지 페이지에서 빌드 결과를 확인합니다.

Git 태그의 버전은 `package.json.version`과 같아야 합니다. 이 저장소의 manifest만 수정해도 자동 등록·배포되지는 않습니다. 최신 요구 사항은 [공식 등록 가이드](https://openupm.com/docs/adding-upm-package)를 확인하세요.

## 등록 후 설치

OpenUPM에서 1.0.0 빌드가 완료되면 소비 Unity 프로젝트에서 실행합니다.

```sh
openupm add com.achive.event-tracer@1.0.0
```

CLI 없이 설치하려면 [영어 가이드의 manifest 예시](publishing.md#installation-after-registration)를 기존 `Packages/manifest.json`의 레지스트리·의존성에 병합하세요. 등록 전에는 README의 Git URL로 설치합니다. `v1.0.0` 태그를 푸시한 뒤에는 URL 끝에 `#v1.0.0`을 붙여 버전을 고정할 수 있습니다.

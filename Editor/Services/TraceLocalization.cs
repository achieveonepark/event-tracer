using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Achieve.EventTracer.Editor
{
    public static class TraceLocalization
    {
        private const string PreferenceKey = "Achieve.Editor.Language";
        private static readonly Dictionary<string, string> KoreanText = new Dictionary<string, string>
        {
            { "Apply", "적용" },
            { "Revert", "되돌리기" },
            { "Enabled", "활성화" },
            { "Timeout Seconds", "제한 시간 (초)" },
            { "Language", "언어" },
            { "Notes", "메모" },
            { "Position", "위치" },
            { "Rotation", "회전" },
            { "Target Id", "대상 ID" },
            { "Action Id", "액션 ID" },
            { "Payload Json", "JSON 인자" },
            { "Seconds", "대기 시간 (초)" },
            { "Start scene", "시작 씬" },
            { "Seed", "시드" },
            { "Use Random Seed", "랜덤 시드 사용" },
            { "Random Seed", "랜덤 시드" },
            { "Description", "설명" },
            { "Ready Signal Id", "준비 신호 ID" },
            { "Ready Timeout Seconds", "준비 제한 시간 (초)" },
            { "Missing Type", "누락된 타입" },
            { "Original Json", "원본 JSON" },
            { "Record", "기록" },
            { "Capture caller", "호출 위치 기록" },
            { "Capture stack trace", "호출 스택 기록" },
            { "Stack trace characters", "호출 스택 문자 수" },
            { "Capture the call stack for new events. Adds recording overhead.", "새 이벤트의 호출 스택을 기록합니다. 기록 비용이 증가합니다." },
            { "Stack trace limit must be 128–65,536 characters.", "호출 스택 제한은 128~65,536자여야 합니다." },
            { "Event limit must be 1–100,000.", "이벤트 수는 1~100,000개여야 합니다." },
            { "Payload limit must be 128–65,536 characters.", "데이터 제한은 128~65,536자여야 합니다." },
            { "Buffer budget must fit one payload and be at most 64M characters.", "버퍼는 데이터 하나를 담을 수 있어야 하며 최대 64M자입니다." },
            { "Info", "정보" },
            { "Warning", "경고" },
            { "Error", "오류" },
            { "Event Tracer", "이벤트 추적기" },
            { "Recorded events and causes", "기록된 이벤트와 원인" },
            { "Play session", "플레이 세션" },
            { "Manual session", "수동 세션" },
            { "Editor session", "에디터 세션" },
            { "LIVE", "실시간" },
            { "IMPORTED", "불러온 캡처" },
            { "PAUSED VIEW", "화면 일시 정지" },
            { "Pause view", "화면 정지" },
            { "Resume view", "화면 재개" },
            { "New session", "새 세션" },
            { "Clear", "비우기" },
            { "Export", "내보내기" },
            { "Import", "불러오기" },
            { "Search event names and payloads", "이벤트 이름과 데이터를 검색합니다" },
            { "All categories", "모든 카테고리" },
            { "All levels", "모든 수준" },
            { "Source", "소스" },
            { "Filter by source object name", "소스 오브젝트 이름으로 필터링합니다" },
            { "Back to live", "실시간으로 돌아가기" },
            { "TIME", "시간" },
            { "FRAME", "프레임" },
            { "EVENT / CAUSE", "이벤트 / 원인" },
            { "SOURCE", "소스" },
            { "Capture limits", "수집 제한" },
            { "Events", "이벤트 수" },
            { "Payload characters", "데이터 문자 수" },
            { "Buffer characters", "버퍼 문자 수" },
            { "Caller location", "호출 위치" },
            { "Capture caller location", "호출 위치 기록" },
            { "Waiting for your first event.\nRun your game or try the Shop Demo.", "첫 이벤트를 기다리고 있습니다.\n게임을 실행하거나 Shop Demo를 사용하세요." },
            { "No events match these filters.", "필터와 일치하는 이벤트가 없습니다." },
            { "No event selected", "선택한 이벤트가 없습니다" },
            { "Select an event to inspect its data, source, and recorded cause.", "이벤트를 선택하면 데이터, 소스와 기록된 원인을 확인할 수 있습니다." },
            { "EVENT DETAILS", "이벤트 상세" },
            { "RECORDED CAUSE", "기록된 원인" },
            { "PAYLOAD SNAPSHOT", "데이터 스냅샷" },
            { "CALL SITE", "호출 위치" },
            { "CALL STACK", "호출 스택" },
            { "No call stack recorded. Enable Capture stack trace for new events.", "기록된 호출 스택이 없습니다. 새 이벤트부터 호출 스택 기록을 활성화하세요." },
            { "SESSION METADATA", "세션 메타데이터" },
            { "Parent belongs to another session or is outside this capture.", "부모 이벤트가 다른 세션에 있거나 캡처 범위 밖에 있습니다." },
            { "Root event · no parent recorded.", "최상위 이벤트 · 기록된 부모 없음" },
            { "No source object supplied.", "소스 오브젝트가 지정되지 않았습니다." },
            { "Select source object", "소스 오브젝트 선택" },
            { "Open code", "코드 열기" },
            { "Open in Event Tracer", "이벤트 추적기로 열기" },
            { "Session metadata", "세션 메타데이터" },
            { "Event preview", "이벤트 미리보기" },
            { "This .etr file could not be imported. Check its source JSON and Console error.", ".etr 파일을 불러오지 못했습니다. 원본 JSON과 Console 오류를 확인하세요." },
            { "Export trace capture", "추적 캡처 내보내기" },
            { "Import trace capture", "추적 캡처 불러오기" }
        };
        private sealed class TextState { public string original; public string displayed; }
        private sealed class ElementState { public readonly Dictionary<string, TextState> values = new Dictionary<string, TextState>(); }
        private sealed class Schedule { public IVisualElementScheduledItem item; }
        private static readonly ConditionalWeakTable<VisualElement, ElementState> Originals = new ConditionalWeakTable<VisualElement, ElementState>();
        private static readonly ConditionalWeakTable<VisualElement, Schedule> Schedules = new ConditionalWeakTable<VisualElement, Schedule>();
        public static bool IsKorean => EditorPrefs.GetString(PreferenceKey, "en") == "ko";
        public static void SetLanguage(bool korean) => EditorPrefs.SetString(PreferenceKey, korean ? "ko" : "en");
        public static void SetText(TextElement element, string english)
        {
            var state = Originals.GetValue(element, _ => new ElementState());
            var translated = Text(english);
            state.values["text"] = new TextState { original = english, displayed = translated };
            element.text = translated;
        }
        public static string Text(string english)
        {
            if (!IsKorean || string.IsNullOrEmpty(english)) return english;
            if (KoreanText.TryGetValue(english, out var translated)) return translated;
            var match = Regex.Match(english, @"^([\d,]+) shown · ([\d,]+) captured · ([\d,]+) dropped$");
            if (match.Success) return $"표시 {match.Groups[1]} · 수집 {match.Groups[2]} · 제외 {match.Groups[3]}";
            match = Regex.Match(english, @"^([\d,]+) events · ([\d,]+) dropped$");
            if (match.Success) return $"이벤트 {match.Groups[1]}개 · 제외 {match.Groups[2]}개";
            match = Regex.Match(english, @"^RECORDED EFFECTS  ·  (\d+)$");
            if (match.Success) return $"기록된 결과 · {match.Groups[1]}개";
            match = Regex.Match(english, @"^([\d,]+) more (events in the trace window|effects in the event list)\.$");
            if (match.Success) return $"나머지 {match.Groups[1]}개는 추적 창에서 확인하세요.";
            return english;
        }
        public static ToolbarMenu CreateSelector()
        {
            var menu = new ToolbarMenu { name = "achieve-language", text = IsKorean ? "한국어" : "English", tooltip = "한국어 / English" };
            menu.menu.AppendAction("한국어", _ => SetLanguage(true), _ => IsKorean ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            menu.menu.AppendAction("English", _ => SetLanguage(false), _ => IsKorean ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Checked);
            return menu;
        }
        public static void Install(VisualElement root)
        {
            if (Schedules.TryGetValue(root, out var previous)) previous.item.Pause();
            Schedules.Remove(root);
            var item = root.schedule.Execute(() => Apply(root)).Every(200);
            Schedules.Add(root, new Schedule { item = item });
            Apply(root);
        }
        public static void Apply(VisualElement root)
        {
            var pending = new Stack<VisualElement>();
            pending.Push(root);
            while (pending.Count > 0)
            {
                var element = pending.Pop();
                foreach (var child in element.Children().ToArray()) pending.Push(child);
                if (element.ClassListContains("achieve-data")) continue;
                if (element is ToolbarMenu menu && menu.name == "achieve-language")
                {
                    menu.text = IsKorean ? "한국어" : "English";
                    continue;
                }
                Transform(element, "tooltip", () => element.tooltip, text => element.tooltip = text);
                // Input values, event payloads and identifiers are deliberately never translated.
                if (element is Label label) Transform(element, "text", () => label.text, text => label.text = text);
                else if (element is Button button) Transform(element, "text", () => button.text, text => button.text = text);
                else if (element is Foldout foldout) Transform(element, "text", () => foldout.text, text => foldout.text = text);
                var property = element.GetType().GetProperty("label");
                if (property != null && property.PropertyType == typeof(string) && property.CanRead && property.CanWrite)
                    Transform(element, "label", () => (string)property.GetValue(element), text => property.SetValue(element, text));
            }
        }
        private static void Transform(VisualElement element, string key, Func<string> read, Action<string> write)
        {
            var current = read();
            if (string.IsNullOrEmpty(current)) return;
            var state = Originals.GetValue(element, _ => new ElementState());
            if (!state.values.TryGetValue(key, out var textState))
            {
                textState = new TextState { original = EnglishOriginal(current) };
                state.values.Add(key, textState);
            }
            else if (current != textState.displayed && current != textState.original) textState.original = EnglishOriginal(current);
            var translated = Text(textState.original);
            textState.displayed = translated;
            if (current != translated) write(translated);
        }
        private static string EnglishOriginal(string current)
        {
            foreach (var entry in KoreanText) if (entry.Value == current) return entry.Key;
            return current;
        }
    }
}

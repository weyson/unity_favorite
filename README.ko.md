# 즐겨찾기（Unity Editor）

**Languages:** [简体中文](README.md) | [繁體中文](README.zh-TW.md) | [English](README.en.md) | [日本語](README.ja.md) | [한국어](README.ko.md) | [Русский](README.ru.md)

Editor 전용 패키지: Project의 에셋/폴더를 즐겨찾기에 추가하고, 분류·접기·빠른 찾기를 지원합니다.

## 설치

### 방법 1: 프로젝트에 포함

이 저장소를 Unity 프로젝트의 `Packages/com.unityfavorite.favorites/` 에 복사합니다.

### 방법 2: Git URL（Package Manager）

```
https://github.com/weyson/unity_favorite.git
```

패키지가 저장소 루트에 있으면 위 URL을 추가하세요. 로컬 경로 `file:D:/workspace/unity_favorite` 도 사용할 수 있습니다.

## 사용법

1. 창 열기: `Window → 常用资源 → 常用资源`
2. **사용 설명**: 툴바 「?」, 또는 `Window → 常用资源 → 使用说明`（설명 창에서 현재 언어 README를 열 수 있음）
3. **추가**: Project에서 드래그, 또는 우클릭 → `添加到常用` / `添加到常用（指定分类）`
4. **분류**: 툴바 「새 분류」; 분류 제목 우클릭으로 이름 바꾸기/삭제
5. **접기**: 화살표로 펼치기/접기（상태 저장）
6. **검색**: 이름/경로 필터; 검색 중 일치 분류가 펼쳐짐
7. **찾기/열기**: 클릭으로 Ping; 더블클릭으로 열기（폴더는 Ping）; 행의 `↗`로 탐색기 열기（파일은 상위 폴더）
8. **언어**: 툴바 Popup（간체/번체/English/日本語/한국어/Русский, EditorPrefs 저장）
9. **정리**: 「유효하지 않음 정리」로 Missing 제거

## 데이터

프로젝트 루트 `UserSettings/FavoritesData.json`（사용자·프로젝트별, 보통 VCS 제외）. GUID 참조로 이동/이름 변경 후에도 유효합니다.

## 지원 범위

- `Assets/` 아래 에셋과 폴더만
- Hierarchy 오브젝트, 컴포넌트, 메뉴 항목, 외부 파일/URL 미지원

## 문서

| 언어 | 파일 |
|------|------|
| 简体中文 | [README.md](README.md) |
| 繁體中文 | [README.zh-TW.md](README.zh-TW.md) |
| English | [README.en.md](README.en.md) |
| 日本語 | [README.ja.md](README.ja.md) |
| 한국어 | [README.ko.md](README.ko.md) |
| Русский | [README.ru.md](README.ru.md) |

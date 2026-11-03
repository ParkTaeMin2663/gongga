# 진행 상황 — 「공가」

두 사람(A, B)과 각자의 Claude가 **같은 상황을 알기 위한 공유 기록**이다.
- 작업을 시작하기 전에 `git pull` → 이 파일을 읽는다.
- 작업을 끝내면 이 파일을 고치고 같이 커밋·푸시한다.
- 게임 설계·규칙이 바뀌면 이 파일이 아니라 **CLAUDE.md**를 고친다.

---

## 이번 주 (11/2 ~ 11/8)

### A (시스템)
| 할 일 | 상태 | 메모 |
|---|---|---|
| ① 물건 집기 | ✅ B 확인(11/03) | `PickupObject.cs`, `Assets/Prefabs/Box`. Test_A 씬 |
| ② 물건 던지기 | ✅ B 확인(11/03) | 들고 있을 때 마우스 오른쪽. 새 액션 `Throw` (InputSystem_Actions) |
| ③ 서랍 | 🟡 코드 완료, 테스트 전 | `DraggableDrawer.cs`. Dresser(Empty, Scale 1, +Z가 앞) 아래 Body + Drawer(Rigidbody Is Kinematic). Test_A에 아직 안 만듦 |
| ④ 차단기 레버 | ⬜ | |
| ⑤ NoiseSystem 뼈대 | ⬜ | B의 셔터·센서등 소리가 이걸 쓴다 |

### B (월드·연출)
| 할 일 | 상태 | 메모 |
|---|---|---|
| 1층 슬래브 + 계단 경사로 | 🟡 B 완료(11/03), A 확인 전 | 층 높이 2.8. 계단실 `Stairwell`: 보이지 않는 경사로(Ramp_A/B, 25°, Mesh Renderer 끔) + 보이는 계단(Steps_A/B, Collider 없음) + 중간 참 |
| 2·3층 복사, 문 이름·잠금, 지붕 | 🟡 B 완료(11/03), A 확인 전 | Floor1을 복사해 Floor2(y 2.8)·Floor3(y 5.6), `Roof`(y 8.4). 잠금: 101~103, 202, 203 |
| 세대 칸막이 (201, 301~304) | 🟡 B 완료(11/03), A 확인 전 | `Assets/Prefabs/Unit` 프리팹(동쪽 칸막이 + 창문 뚫린 뒷벽). 4호는 Office 벽 |
| 302호 창문 불빛, "공가" 표시 | 🟡 B 완료(11/03), A 확인 전 | 302호 복도 쪽 창 + Unlit 노란 Quad + Point Light. 스프레이 텍스처 `graffiti_01~03.png`, `gongga_spray.png` (문에 Quad, Collider 없음) |
| 벽 텍스처 | 🟡 B 완료(11/03), A 확인 전 | 재질 복사 + Tiling(벽 크기 ÷ 2)으로 늘어남 해결 (강의 03) |

상태: ⬜ 안 함 / 🟡 진행 중 / ✅ 완료(상대방이 확인함)

---

## A → B 배치 요청 (SampleScene은 B만 수정)
프리팹이 완성되면 A가 여기에 적고, B가 배치한 뒤 지운다.

| 프리팹 | 놓을 곳 | 설정 |
|---|---|---|
| (아직 없음) | | |

---

## 정해진 것 (대화에서 결정된 사항)
- 11/02: 역할 분담, 11/22 기능 동결 (CLAUDE.md 2장)
- 11/02: 게임 시작은 단지 바깥 (CLAUDE.md 4장 맵 구성)
- 11/02: 무료 에셋 최대 활용 (교수님). Import 시 `.cs` 파일은 빼고 가져온다. Credits.txt 기록
- 11/02: 괴물 외형 후보 = Sketchfab "Country Road Creature Rig" (CC BY). 애니메이션 재생에 Animator(강의 외) 사용 예정. 적용은 11/16 주
- 11/03: 층 높이 2.8 (1층 0 / 2층 2.8 / 3층 5.6 / 지붕 8.4)
- 11/03: 계단은 "밟는 건 경사로, 보이는 건 계단" 방식. 계단 걸림은 Player Capsule에 Physics Material `PlayerSlip`(마찰 0, Minimum), 경사로에 마찰 1(Maximum)
- 11/03: 벽 텍스처 자동 Tiling 스크립트는 보류, 재질 Tiling 수동 조절로 결정
- 문은 `LocksLook = true`(잡는 동안 시점 고정), 들고 다니는 물건은 `false` — `IInteractable`에 추가됨

## 알려진 문제
- (가끔) 상자를 들고 고개를 빠르게 돌린 뒤 놓으면 멀리 튕겨 나감 → 상자가 플레이어 몸을 통과하며 겹쳐서 생기는 현상. 다시 생기면 `Physics.IgnoreCollision` + `maxDepenetrationVelocity`(강의 외)로 고치기로 함
- 벽 텍스처가 벽 길이에 따라 늘어나 보임 → 자동 Tiling 스크립트 검토 중 (강의 외 API, 결정 전)

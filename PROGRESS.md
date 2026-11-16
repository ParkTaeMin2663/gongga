# 진행 상황 — 「공가」

두 사람(A, B)과 각자의 Claude가 **같은 상황을 알기 위한 공유 기록**이다.
- 작업을 시작하기 전에 `git pull` → 이 파일을 읽는다.
- 작업을 끝내면 이 파일을 고치고 같이 커밋·푸시한다.
- 게임 설계·규칙이 바뀌면 이 파일이 아니라 **CLAUDE.md**를 고친다.

---

## 지금 (11/15 ~ 11/22 기능 동결)
11/9 주 A 일이 밀려서 11/16 주 일과 합쳐 순서대로 진행한다. (11/15 결정)
B도 11/9 주 일(2층 연출·단서 UI·지하 맵·바깥 1차)을 못 해서 11/16 주에 합쳐 진행한다. (11/16)

### A (시스템)
| 순서 | 할 일 | 단계 | 상태 | 메모 |
|---|---|---|---|---|
| 1 | 장롱 숨기 | 목표 | 🟡 A 테스트 완료(11/15), B 확인 전 | `HideSpot.cs`(장롱) + `PlayerHide.cs`(Player). 장롱을 보고 **E 1초 꾹** → 문이 열리고 걸어 들어감 → 문 닫힘(12° 틈으로 엿봄, Peek Angle). E 한 번 → 나옴. 안에서는 이동·잡기 불가, 고개만. 모델 문짝을 DoorA/DoorB로 분리, 경첩은 문 모양에서 자동 계산. 모델 안 선반은 지움. 문 연결이 없으면 화면 어두워짐 방식으로 동작. 들어갈 때 3m 소리. 괴물은 `PlayerHide.IsHidden` 확인 (11/15: 문 닫히기 전, 발이 안에 들어간 순간부터 숨은 것으로 변경 — 들어가는 동안 잡히던 문제). 모델: Old Russian wardrobe 1983 (CC BY). 문 달린 모델이면 `doorLeft/Right` 연결 시 문이 실제로 열리는 방식도 지원. Interact 액션 = Press(Press And Release). Main Camera Near Clip 0.05 |
| 2 | 괴물 뼈대 + 소리 감지 | 필수 + 목표 | 🟡 A 테스트 완료(11/15), B 확인 전 | `Monster/MonsterAI.cs`, `Assets/Prefabs/Monster`(임시 빨간 캡슐 2.4m). 순찰(patrolPoints) → 소리 들으면 확인 → 2.5m 안 기척이면 추격(3.5m/s, 걷기 3 < 괴물 < 달리기 5.5) → 8m 밖이면 놓침 → 수색 4초 → 순찰. **장롱에 숨으면 그 장롱 앞을 8초 서성이며 가끔 쳐다봄(헐떡이면 시간 초기화) → 순찰**. 1.1m 안이면 잡힘 → 씬 재시작. 씬에 이름 `Player`·`NoiseSystem` 필요. 이동은 **NavMeshAgent**(07장)로 교체 완료(11/15). 다른 층 소리는 무시 |
| 3 | 괴물 이동 NavMeshAgent로 | 필수 | 🟡 A 테스트 완료(11/15, 07장 자료로 앞당김), B 확인 전 | Monster 프리팹에 Nav Mesh Agent(Radius 0.3, Height 2.4). **Navigation 창 Agents > Humanoid Radius 0.3**(문 틈 1m를 지나가게, ProjectSettings에 저장) + NavMesh Modifier(Ignore From Build, 자기 자리 구멍 방지). Rigidbody는 Is Kinematic. Door 프리팹에도 NavMesh Modifier(Ignore From Build) → 닫힌 문도 길로 굽힘(문 앞 멈춤·부수기는 5번). **SampleScene에도 NavMesh Surface Bake 필요(B와 상의)** |
| 4 | 사망·체크포인트 | 목표 | 🟡 A 테스트 완료(11/15), B 확인 전 | `Core/Checkpoint.cs`(Is Trigger 구역, `public static` 저장값, order 큰 것만 저장) + `Player/PlayerDeath.cs`(잡히면 괴물 쪽으로 고개 → 붉게 → 까맣게(코루틴) → 씬 재로드 → 마지막 체크포인트 위치·방향에서 시작). Player 프리팹 Tag = `Player`. 위치만 되돌리고 물건·문 상태는 안 되돌림. **B 연출은 `Checkpoint.savedOrder`로 이미 지난 장면을 건너뛸 수 있음**. **나중에(11/22 전):** 사망 시 점프스케어(괴물 모델이 카메라로 덮침 + 비명, 7번 외형과 함께) + "다시 시작 / 그만하기" 선택 화면(B 타이틀 화면과 함께, UI 방식 결정·커서 보이기 필요). `PlayerDeath.DeathRoutine` 3단계(자동 재시작)를 바꾸면 됨 |
| 5 | 문 부수기 | 목표 | 🟡 A 테스트 완료(11/15), B 확인 전 | 괴물 앞 1.2m에 닫힌 문(Raycast) → 추격 중·잠긴 문이면 0.7초마다 쾅(3초) 후 **경첩 Destroy + 문짝 날림**(12m 소리), 평소엔 문을 밀어서 엶(안 열리면 부숨). `DraggableDoor`에 PushOpen/Knock/Break, 소리 `door_knock`(Basement door slam 한 방, 저음 강조판 `door_knock_b`), `door_break`(문 걷어차 부서지는 녹음 + 나무 박살, 대체 `door_break_b`)(자식 `BreakSoundPoint`에서 재생 → 부서질 때 떼어 놓아 그 자리에서 들림). 두 소리 Min Distance 6·도플러 0. 부서진 문짝이 떨어져 부딪히면 `door_fall` 쿵(OnCollisionEnter, 세기에 따라 볼륨). 부서진 문은 못 잡음. 문 틈(1.0m)에 덜 걸리게 **Player Capsule Radius 0.4 → 0.3**(11/15) |
| 6 | 손전등 배터리 | 목표 | 🟡 A 테스트 완료(11/15), B 확인 전 | `PlayerFlashlight.cs`: 켜면 0.5/초 소모(100 → 3분 20초), 20% 아래 어두워지며 깜빡, 0이면 안 켜짐. 왼쪽 아래 배터리 막대(OnGUI). 딸깍 소리. **B 연출용 `FlickerOut(초)`** (깜빡이다 꺼짐, 코루틴), `GiveFlashlight()`(관리사무소에서 얻을 때, `hasFlashlight` 끄고 시작). 건전지 `Assets/Prefabs/Battery`(+50). **설계대로(11/15): Player는 손전등 없이 시작(`hasFlashlight` 끔)** → 관리사무소 `FlashlightPickup`을 **E로 주워** 얻음(Poly Haven Plastic Torch 6V). 줍는 물건은 `ItemPickup.cs`(종류: Flashlight/Battery) + Player의 `PlayerPickup.cs`(바라보면 "[E] 손전등 줍기"). **얻으면 왼손에 들림**(Main Camera > `HeldFlashlight`, 불빛이 렌즈에서 나옴, 걸으면 `FlashlightSway`로 살짝 흔들림·뛰면 크게). 건전지 모델 D Battery(CC BY). 체크포인트가 손전등 보유·배터리 양도 기억 → 죽어도 안 잃음 |
| 7 | 괴물 외형 (무료 모델) | 여유 | ⬜ | 후보: Country Road Creature Rig (CC BY). 넣을 때 4번 점프스케어도 같이 |
| 8 | 공통 UI 스타일 | 필수 | ⬜ | `UI/UIStyle.cs`(public static 글꼴·색·GUIStyle) + 무료 한글 글꼴(OFL, Credits 기록). A 화면 표시(배터리·안내 문구·저장됨·사망)를 먼저 바꾸고, B는 단서·타이틀·엔딩 화면에서 가져다 씀 |
| + | 숨소리 | 추가 | 🟡 A 테스트 완료(11/15), B 확인 전 | `Player/PlayerBreath.cs`. Player 프리팹에 Loop Audio Source 2개(평소 `breath_calm_a`, 헐떡임 `breath_heavy_b`). 뛰면 숨참 0→1(5초), 쉬면 10초에 가라앉음, 두 소리를 섞음. 장롱 속에서 숨참 0.5 넘으면 1.5초마다 2.5m 소리(괴물이 확인하러 옴). 대체 후보 `breath_calm_b`, `breath_heavy_a` |

### B (월드·연출)
11/15 A가 정리. 순서대로 하면 A 기능과 바로 이어진다. 시작 전 `git pull`, SampleScene은 작게 나눠 자주 커밋.

| 순서 | 할 일 | 단계 | 상태 | 메모 |
|---|---|---|---|---|
| 1 | SampleScene NavMesh Bake | 필수 | ✅ A 확인(11/16) | 아래 배치 요청 표. 없으면 괴물이 못 움직임. 맵 바꿀 때마다 다시 Bake |
| 2 | A 프리팹 배치 | 필수 | ✅ A 확인(11/16) | 배치 요청 표 전부: Monster+순찰 지점, 장롱 3, 체크포인트 3, Breaker, Dresser·Nightstand·Box |
| 3 | A 기능 확인 → ✅ | 필수 | ⬜ | 장롱 숨기, 괴물(소리·추격·서성이기), 문 부수기, 사망·체크포인트, 숨소리, 서랍·레버 |
| 4 | UI 방식 A와 결정 | 필수 | ✅ A 확인(11/16): **OnGUI + 공통 스타일** | A 쪽 화면 표시(배터리·숨기·사망·줍기)가 전부 OnGUI라 맞춤. 단서 UI도 OnGUI로 작성함. 타이틀·엔딩·사망 선택 화면도 OnGUI로 (커서는 `Cursor.lockState = None`, `visible = true`로 보이기). A가 반대면 PROGRESS에 적어 주기 |
| 5 | 2층 연출 (셔터·센서등·실루엣·"숨어라") | 필수 | ✅ A 확인(11/16) | `Story/Floor2Event.cs` (빈 오브젝트 `Floor2Event`). 레버 IsOn → 셔터가 0.4m 올라가다 걸림 → 센서등이 계단 쪽부터 하나씩 → 204호 앞 실루엣 → `FlickerOut` + 센서등 꺼짐 → "숨어라" → 실루엣이 길(Path)을 따라 1m/s로 기어 옴(201호 앞에서 4초 킁킁) → 숨지 않고 1.3m 안이면 `PlayerDeath.Die` → 계단으로 사라지면 셔터 끝까지. 셔터는 localScale.y를 줄여 위로 말려 올라감(3층 바닥 안 뚫음). 셔터·센서등 `Emit(…, 20)`. 실루엣은 MonsterAI 없는 별도 오브젝트(임시 검은 캡슐, 괴물 외형 나오면 교체) |
| 6 | 단서 UI (일기장·사진·카세트) | 필수 | ✅ A 확인(11/16) | `Story/ClueItem.cs`(단서 물건: 종류·제목·pages·image·audioClip) + `UI/ClueReader.cs`(빈 오브젝트 `ClueReader` + Audio Source). 바라보면 "[E] 일기장 읽기", E → 누런 종이 화면, E로 다음 장/닫기. 읽는 동안 걷기·시점·잡기 멈춤. **Player 프리팹은 안 건드림**: E는 `Keyboard.current.eKey`로 직접 읽음(06장 Mouse.current 방식), Raycast는 Main Camera에서 |
| 7 | 단지 바깥 + 바깥에서 시작 | 필수 | ⬜ | 06장 Terrain 가능. Player와 체크포인트 1번을 바깥 시작점으로 옮기기 → NavMesh 다시 Bake |
| 8 | 지하 맵 (보일러실·제사상·비상구) | 필수 | ⬜ | 체크포인트 4번 + 괴물 최종 추격용 순찰 지점 (A에게 위치 알려 주기) |
| 9 | 3장 시작 시 Monster 켜기 | 필수 | ✅ A 확인(11/16) | `Story/ActivateOnEnter.cs`: 3층 계단 위 Trigger 구역 `Chapter3Start`에 들어오면 Targets(Monster) SetActive(true) |
| 10 | 다시 시작 시 지난 연출 건너뛰기 | 목표 | ✅ A 확인(11/16) | `Floor2Event.skipFromCheckpoint`(3 이상이면 셔터 열림·실루엣 없음), `ActivateOnEnter.activeFromCheckpoint`(3 이상이면 Monster 바로 켬). 2층 연출 중 잡히면 체크포인트 2에서 연출 다시 |
| 11 | 스토리 연출·엔딩 | 필수 | ⬜ | 302호 동생 흔적, 지하 제사상, 탈출 엔딩 |
| 12 | 타이틀·엔딩 화면 | 필수 | ⬜ | A의 "다시 시작 / 그만하기" 화면과 같이 |
| 13 | 사운드 (배경음·환경음) | 목표 | ⬜ | 바람, 눈, 건물 삐걱임 등. 출처 Credits.txt |
| — | **11/22 기능 동결** | | | 이후 버그 수정·통플레이만 |

---

## 지난주 (11/2 ~ 11/8)

### A (시스템)
| 할 일 | 상태 | 메모 |
|---|---|---|
| ① 물건 집기 | ✅ B 확인(11/03) | `PickupObject.cs`, `Assets/Prefabs/Box`. Test_A 씬 |
| ② 물건 던지기 | ✅ B 확인(11/03) | 들고 있을 때 마우스 오른쪽. 새 액션 `Throw` (InputSystem_Actions) |
| ③ 서랍 | 🟡 A 테스트 완료(11/07), B 확인 전 | `DraggableDrawer.cs`, `Assets/Prefabs/Dresser`, `Assets/Prefabs/Nightstand`(무료 모델). 가져온 모델은 서랍 부품에 Box Collider + Rigidbody + DraggableDrawer, 방향은 Open Direction으로 맞춤. Dresser(Empty, Scale 1, +Z가 앞) 아래 Body + Drawer(Rigidbody Use Gravity 끔·Is Kinematic). 당기면 열림, 최대 0.35m |
| ④ 차단기 레버 | 🟡 A 테스트 완료(11/07), B 확인 전 | `DraggableLever.cs`. `Assets/Prefabs/Breaker`. Breaker(Empty) 아래 Box + Lever(Empty, 회전축) + 자식 Handle(Cube). 재질은 기존 door_material(몸통)·maker material(손잡이). 마우스 위로 밀면 올라감, 끝까지 올리면 `IsOn = true`(한 번 켜지면 고정). B 셔터 연출은 `lever.IsOn` 확인 |
| ⑤ NoiseSystem 뼈대 | 🟡 A 테스트 완료(11/07), B 확인 전 | `Noise/NoiseSystem.cs`, 씬에 빈 오브젝트 `NoiseSystem`(이름 정확히) 1개. 소리 내기: Start()에서 `GameObject.Find("NoiseSystem")` → `Emit(위치, 크기m, 이름)`. 연결됨: 달리기 6m(걸음마다 0.35초, 걷기는 실제 발소리만), 물건 부딪힘 8m(2m/s 이상), 차단기 15m. 괴물은 `NoiseCount` 변화 + `CanHear(위치)` + `LastPosition` 사용 예정. B 셔터·센서등은 20m 권장. 실제 발소리: Player의 Audio Source + `Assets/Audio/Footsteps`(Kenney CC0), 걷기 작게·달리기 크게. 효과음(11/07): Box 부딪힘, 레버 철컥, 서랍·문 열기/닫기/잠김 — 프리팹에 Audio Source로 들어 있음(Spatial Blend 3D). `DraggableDrawer`·`DraggableDoor`에 Open/Close/Locked Sound 칸 추가. 소리 파일 `Assets/Audio/`(Kenney CC0, Pixabay), Credits 기록. 안 쓰는 변형 파일(_001~004 등)은 나중에 정리 |

### B (월드·연출)
| 할 일 | 상태 | 메모 |
|---|---|---|
| 1층 슬래브 + 계단 경사로 | ✅ A 확인(11/16) | 층 높이 2.8. 계단실 `Stairwell`: 보이지 않는 경사로(Ramp_A/B, 25°, Mesh Renderer 끔) + 보이는 계단(Steps_A/B, Collider 없음) + 중간 참 |
| 2·3층 복사, 문 이름·잠금, 지붕 | ✅ A 확인(11/16) | Floor1을 복사해 Floor2(y 2.8)·Floor3(y 5.6), `Roof`(y 8.4). 잠금: 101~103, 202, 203 |
| 세대 칸막이 (201, 301~304) | ✅ A 확인(11/16) | `Assets/Prefabs/Unit` 프리팹(동쪽 칸막이 + 창문 뚫린 뒷벽). 4호는 Office 벽 |
| 302호 창문 불빛, "공가" 표시 | ✅ A 확인(11/16) | 302호 복도 쪽 창 + Unlit 노란 Quad + Point Light. 스프레이 텍스처 `graffiti_01~03.png`, `gongga_spray.png` (문에 Quad, Collider 없음) |
| 벽 텍스처 | ✅ A 확인(11/16) | 재질 복사 + Tiling(벽 크기 ÷ 2)으로 늘어남 해결 (강의 03) |

상태: ⬜ 안 함 / 🟡 진행 중 / ✅ 완료(상대방이 확인함)

---

## A → B 배치 요청 (SampleScene은 B만 수정)
프리팹이 완성되면 A가 여기에 적고, B가 배치한 뒤 지운다. (11/16: Dresser·Box·Breaker·장롱 3·NavMesh·Monster·손전등·건전지·체크포인트 1~3 배치 확인 → 지움)

| 프리팹 | 놓을 곳 | 설정 |
|---|---|---|
| `Nightstand` | 세대 안 침실 자리 (서랍 단서를 넣기 좋음). Dresser 대신 써도 됨 | 무료 모델(Poly Haven, CC0). 서랍이 앞으로 열리게 Open Direction 맞춰 둠. 회전은 서랍 앞면이 방 안쪽을 보게 |
| `Checkpoint` (남은 것) | 1~3번 배치 완료(11/16). **단지 바깥이 생기면 1번을 바깥 시작점으로 옮기기, 지하가 생기면 4번 추가**(Order = 4) | 파란 화살표가 다시 시작할 때 보는 방향 |

---

## 정해진 것 (대화에서 결정된 사항)
- 11/02: 역할 분담, 11/22 기능 동결 (CLAUDE.md 2장)
- 11/02: 게임 시작은 단지 바깥 (CLAUDE.md 4장 맵 구성)
- 11/02: 무료 에셋 최대 활용 (교수님). Import 시 `.cs` 파일은 빼고 가져온다. Credits.txt 기록
- 11/02: 괴물 외형 후보 = Sketchfab "Country Road Creature Rig" (CC BY). 애니메이션 재생에 Animator(강의 외) 사용 예정. 적용은 11/16 주
- 11/03: 층 높이 2.8 (1층 0 / 2층 2.8 / 3층 5.6 / 지붕 8.4)
- 11/03: 계단은 "밟는 건 경사로, 보이는 건 계단" 방식. 계단 걸림은 Player Capsule에 Physics Material `PlayerSlip`(마찰 0, Minimum), 경사로에 마찰 1(Maximum)
- 11/03: 벽 텍스처 자동 Tiling 스크립트는 보류, 재질 Tiling 수동 조절로 결정
- 11/07: 가구는 Poly Haven(CC0) 모델 사용. 모델은 `Assets/Models/`, 재질은 텍스처로 직접 만든 URP Lit. 서랍 분리된 후보: Vintage Wooden Drawer 01(서랍 6칸, 관리사무소용), Drawer Cabinet, Painted Wooden Cabinet
- 11/07: **Player는 프리팹**(`Assets/Prefabs/Player`, 자식 Main Camera·Flashlight 포함). SampleScene·Test_A 둘 다 이 프리팹 사용. Player 수정은 A가 프리팹에서 한다(씬 수정 불필요). Main Camera 이름·태그 유지
- 11/15: 강의자료 05·06·07장을 CLAUDE.md 6장에 반영. Raycast·RotateAround·Trigger·NavMesh·코루틴·Particle·Tag·static은 이제 강의 내용
- 11/07: A가 B에게 알리고 SampleScene 수정 — Player 프리팹화 + 발소리 Audio Source, `NoiseSystem` 배치 완료
- 문은 `LocksLook = true`(잡는 동안 시점 고정), 들고 다니는 물건은 `false` — `IInteractable`에 추가됨

- 11/16: **UI는 OnGUI(강의 04장) + A·B 공통 스타일(글꼴·색)**. Canvas/TMP는 강의 외 + 다시 만들어야 해서 안 씀. 타이틀·엔딩은 이미지 위주로. 공통 스타일은 A가 `UI/UIStyle.cs`로 만든다

## 알려진 문제
- (가끔) 상자를 들고 고개를 빠르게 돌린 뒤 놓으면 멀리 튕겨 나감 → 상자가 플레이어 몸을 통과하며 겹쳐서 생기는 현상. 다시 생기면 `Physics.IgnoreCollision` + `maxDepenetrationVelocity`(강의 외)로 고치기로 함

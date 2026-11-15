# 진행 상황 — 「공가」

두 사람(A, B)과 각자의 Claude가 **같은 상황을 알기 위한 공유 기록**이다.
- 작업을 시작하기 전에 `git pull` → 이 파일을 읽는다.
- 작업을 끝내면 이 파일을 고치고 같이 커밋·푸시한다.
- 게임 설계·규칙이 바뀌면 이 파일이 아니라 **CLAUDE.md**를 고친다.

---

## 지금 (11/15 ~ 11/22 기능 동결)
11/9 주 A 일이 밀려서 11/16 주 일과 합쳐 순서대로 진행한다. (11/15 결정)

### A (시스템)
| 순서 | 할 일 | 단계 | 상태 | 메모 |
|---|---|---|---|---|
| 1 | 장롱 숨기 | 목표 | 🟡 A 테스트 완료(11/15), B 확인 전 | `HideSpot.cs`(장롱) + `PlayerHide.cs`(Player). 장롱을 보고 **E 1초 꾹** → 문이 열리고 걸어 들어감 → 문 닫힘(12° 틈으로 엿봄, Peek Angle). E 한 번 → 나옴. 안에서는 이동·잡기 불가, 고개만. 모델 문짝을 DoorA/DoorB로 분리, 경첩은 문 모양에서 자동 계산. 모델 안 선반은 지움. 문 연결이 없으면 화면 어두워짐 방식으로 동작. 들어갈 때 3m 소리. 괴물은 `PlayerHide.IsHidden` 확인. 모델: Old Russian wardrobe 1983 (CC BY). 문 달린 모델이면 `doorLeft/Right` 연결 시 문이 실제로 열리는 방식도 지원. Interact 액션 = Press(Press And Release). Main Camera Near Clip 0.05 |
| 2 | 괴물 뼈대 + 소리 감지 | 필수 + 목표 | 🟡 A 테스트 완료(11/15), B 확인 전 | `Monster/MonsterAI.cs`, `Assets/Prefabs/Monster`(임시 빨간 캡슐 2.4m). 순찰(patrolPoints) → 소리 들으면 확인 → 2.5m 안 기척이면 추격(3.5m/s, 걷기 3 < 괴물 < 달리기 5.5) → 8m 밖·장롱 숨으면 놓침 → 수색 4초 → 순찰. 1.1m 안이면 잡힘 → 씬 재시작. 씬에 이름 `Player`·`NoiseSystem` 필요. 이동은 **NavMeshAgent**(07장)로 교체 완료(11/15). 다른 층 소리는 무시 |
| 3 | 괴물 이동 NavMeshAgent로 | 필수 | 🟡 A 테스트 완료(11/15, 07장 자료로 앞당김), B 확인 전 | Monster 프리팹에 Nav Mesh Agent(Radius 0.5, Height 2.4) + NavMesh Modifier(Ignore From Build, 자기 자리 구멍 방지). Rigidbody는 Is Kinematic. Door 프리팹에도 NavMesh Modifier(Ignore From Build) → 닫힌 문도 길로 굽힘(문 앞 멈춤·부수기는 5번). **SampleScene에도 NavMesh Surface Bake 필요(B와 상의)** |
| 4 | 사망·체크포인트 | 목표 | ⬜ | 처음엔 SceneManager.LoadScene 재시작(강의 04) |
| 5 | 문 부수기 | 목표 | ⬜ | |
| 6 | 손전등 배터리 | 목표 | ⬜ | |
| 7 | 괴물 외형 (무료 모델) | 여유 | ⬜ | 후보: Country Road Creature Rig (CC BY) |

### B (월드·연출) — 11/9 주 계획
| 할 일 | 상태 | 메모 |
|---|---|---|
| 2층 연출 (셔터·센서등·실루엣) | ⬜? | 레버 `IsOn` 사용, 장롱(201호) 필요 |
| 단서 UI | ⬜? | OnGUI(강의) / Canvas(강의 외) 중 결정 필요 |
| 지하 맵, 바깥 1차 | ⬜? | |
| (11/16 주) 스토리 연출, 엔딩, 타이틀·엔딩 화면, 사운드 | ⬜ | |

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
| `Dresser` | 303호 안 (서랍 단서 자리). 다른 세대에 소품으로 더 놓아도 됨 | 파란 화살표(+Z)가 방 안쪽(플레이어 쪽)을 향하게 회전. Scale은 1 유지. 잠글 때는 사본의 Drawer에서 Is Locked 체크 |
| `Box` | 3층 복도·세대 곳곳 2~3개 (던져서 괴물 유인용) | 그대로 배치. 크기는 바꿔도 됨 |
| `Nightstand` | 세대 안 침실 자리 (서랍 단서를 넣기 좋음). Dresser 대신 써도 됨 | 무료 모델(Poly Haven, CC0). 서랍이 앞으로 열리게 Open Direction 맞춰 둠. 회전은 서랍 앞면이 방 안쪽을 보게 |
| `Breaker` | 2층 계단 옆 벽 (분전함). 높이 y ≈ 2.8 + 1.3 | 파란 화살표(+Z)가 복도 쪽(플레이어 쪽)을 향하게 회전. Scale 1 유지. 셔터 연출 스크립트에서 자식 `Lever`의 `DraggableLever.IsOn`을 확인 |
| `Wardrobe` (201호) | `Wardrobe_201` 월드 Position **(9, 2.8, 7.93)**, Rotation **(0, 180, 0)** | 2장 숨기 연출용. 숨으면 문틈 정면으로 201호 현관이 보임. Hierarchy 맨 바깥(부모 없음)에 두고 값 그대로 입력 |
| `Wardrobe` (301호) | `Wardrobe_301` **(15.63, 5.6, 5.5)**, Rotation **(0, -90, 0)** | 동쪽 칸막이 벽에 붙어 서쪽을 봄. 일기장은 근처에 |
| `Wardrobe` (304호) | `Wardrobe_304` **(-10, 5.6, 7.93)**, Rotation **(0, 180, 0)** | 추격 중 숨는 두 번째 장롱 |
| **NavMesh Bake** | SampleScene에 빈 오브젝트 `NavMesh` 하나 (위치 상관없음) | Add Component → **NavMesh Surface** → Collect Objects = **All** → **Bake** (07장 47쪽). 맵(벽·바닥·계단·가구)을 바꿀 때마다 다시 Bake. 생기는 `SampleScene/NavMesh-NavMesh.asset`도 커밋 |
| `Monster` | 3층. 시작 위치 304호 안 **(-12, 5.6, 5)**, Rotation (0, 90, 0) | 순찰 지점: 빈 오브젝트 `Patrol_1` (-12, 5.6, 5) / `Patrol_2` (-12, 5.6, 1) / `Patrol_3` (12, 5.6, 1) / `Patrol_4` (2, 5.6, 1) 을 Monster의 Patrol Points에 순서대로. **3장 전에는 Monster 오브젝트를 꺼 두고, 3장 시작 연출에서 켜기** (2층 연출용 실루엣은 별도) |

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

## 알려진 문제
- (가끔) 상자를 들고 고개를 빠르게 돌린 뒤 놓으면 멀리 튕겨 나감 → 상자가 플레이어 몸을 통과하며 겹쳐서 생기는 현상. 다시 생기면 `Physics.IgnoreCollision` + `maxDepenetrationVelocity`(강의 외)로 고치기로 함

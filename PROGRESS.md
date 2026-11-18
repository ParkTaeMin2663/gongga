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
| 7 | 괴물 외형 (무료 모델) | 여유 | 🟡 모델·동작 넣음(11/18), A 테스트 전 | **Cursed Rune Demon**(Sketchfab, CC BY, 부적 글씨가 새겨진 괴물 — "뒤틀린 집 지킴이 신") 결정. 뼈대가 Mixamo 형식이라 **Humanoid** + Mixamo 동작(Walk·Run·LookAround·BashDoor·Scream) + 모델 자체 Idle. `Assets/Models/Monster/`(모델·동작·`MonsterAnimator.controller`). Monster 프리팹에 자식 `Model`(1.05배, 키 약 1.9m)로 들어가고 빨간 캡슐(Body·Face_Front)은 꺼 둠(충돌·길찾기는 그대로). `MonsterAI`가 속도·상태로 동작 고름([강의 외] Animator.CrossFade, 11/18 허락). 2층·3층 괴물 공통. 다음: 사망 점프스케어 |
| 8 | 공통 UI 스타일 | 필수 | 🟡 스크립트 완료(11/16), A 테스트 전 | `UI/UIStyle.cs` — **Player 프리팹에 붙어 있음**(글꼴 연결됨). 글꼴 **Noto Serif KR(본명조) Regular/Bold/Black**(`Assets/Fonts`, OFL, Credits 기록) 하나로 통일. 색: `UIStyle.white/yellow/red/paper/gray`. 쓰는 법(OnGUI 안): `UIStyle.Prompt("[E] 일기장 읽기")`(조준점 아래, [E] 노란색), `UIStyle.Label(rect, 글자, UIStyle.Title(40))`(그림자 있는 글자), 스타일만: `UIStyle.Body(22, UIStyle.paper)` / `Strong` / `Title`. 크기는 1080p 기준 숫자를 넣으면 화면 높이에 맞춰 바뀜. A 쪽 적용 완료: 줍기·숨기 안내, E 막대, "저장됨". **B: `ClueReader`(안내 문구·종이 글자)를 UIStyle로 바꿔 주기.** `Floor2Event`의 "숨어라"는 A가 UIStyle로 바꿈(11/16, 크기 36 → `hideHintSize` 120, 본명조 Black·그림자, 사용자 요청), 타이틀·엔딩도 UIStyle 사용. 제출 전 `NoiseSystem`·`MonsterAI`의 Show Debug 끄기 |
| 9 | 사망 점프스케어 + "다시 시작/그만하기" | 목표 | 🟡 스크립트 완료(11/18), A 테스트 전 | `PlayerDeath`: 잡히면 괴물 쪽으로 0.15초에 고개 → **괴물이 얼굴 앞 0.55m로 덮침**(NavMeshAgent 잠깐 끔, MonsterAI가 Scream 동작) + 화면 흔들림 1초 → 뚝 까매짐 → **"잡혔다" + 다시 시작 / 그만하기**(UIStyle, 마우스 커서 보임, 올리면 노란색). 다시 시작 = 체크포인트, 그만하기 = 종료(에디터는 Play 멈춤). 비명 소리 칸 `jumpscareSound`는 10번에서 채움. Use Jumpscare 끄면 예전 방식. 2층 괴물은 레버를 올리는 순간부터 복도 끝에 서 있음(11/18) |
| 10 | 배경음·긴장 음악 + 괴물 소리 | 목표 | 🟡 3차 수정(11/18), A 테스트 전 | ① **`Core/TensionMusic.cs`**(Player 프리팹 + 2D Loop Audio Source 2개): **배경음 `ambient_loop`(게임 시작부터 계속, 0.35)**, **"숨어라" = 긴박한 음악 + 심장 소리를 섞은 `hide_heartbeat_loop`(1분 42초, 11/18 교체)**(Floor2Event `PlayHide/StopHide`), **추격 = `chase_loop`**(MonsterAI `PlayChase/StopChase`, 우선). 긴장 음악이 나오면 배경음은 0.08로 줄어듦. 서서히 켜짐 1.2초·꺼짐 4초. B: 배경음을 바꾸려면 Player 프리팹 TensionMusic의 Ambient Clip ② **괴물 소리**(Monster 프리팹 `voiceSource`, 3D Loop): 숨소리 `monster_breath_loop`(0.7), **킁킁 `monster_sniff_loop`(1.0, 무음 구간 잘라냄, 처음부터 재생 — 장롱 앞에서 안 들리던 문제)**, 추격 포효 `monster_roar`, 문 부수기 으르렁 `monster_growl`, 점프스케어 비명 `monster_scream`. 모두 Pixabay(Credits). ③ **`Core/RoarScare.cs`**(Player 프리팹): "숨어라" 순간 괴물 쪽에서 포효 → 그 뒤 **약 3분(180±40초)마다 깜짝 포효**(괴물 방향, 없으면 아래층 어딘가, 플레이어 7m 안으로 당겨 재생, 추격 중엔 건너뜀, 다시 시작해도 계속) `monster_roar`·`monster_roar_far` |
| 11 | 아파트 내부 재질 (벽지·장판·천장·문·선반) | 목표 | ⬜ 다음에 먼저 (11/18 사용자 요청) | 1980년대 한국 아파트 느낌의 **무료 텍스처(Poly Haven·ambientCG 등 CC0)** 를 찾아 URP Lit 재질로 만든다 → `Assets/Materials/Interior/`. 후보: 낡은 꽃무늬 벽지(뜯긴 것), 노란 비닐 장판, 석고 천장(얼룩), 나무 문·문틀, 선반·가구. 원래 B 담당(텍스처)이라 **재질은 A가 만들고, SampleScene 적용은 B와 상의**(A가 적용하면 PROGRESS에 알리기). Credits 기록 |
| 12 | 기본 큐브 물건 → 진짜 모델로 교체 | 목표 | ⬜ (11/18 사용자 요청) | 지금 기본 Cube인 A 프리팹: **`Box`(상자), `Breaker`(분전함 몸통·손잡이), `Door`(문짝), `Dresser`(Body·Drawer)**. Poly Haven·Sketchfab 등 무료 모델로 겉모습만 바꾸고 **스크립트·Collider·Rigidbody·이름은 그대로** 둬서 씬 배치가 안 깨지게 한다(모델은 자식으로 넣고 Cube Mesh Renderer만 끔 — Monster 외형과 같은 방식). 프리팹만 고치므로 SampleScene 수정 불필요. Credits 기록 |
| 13 | 방 안 꾸미기용 소품 프리팹 | 목표 | ⬜ (11/18 사용자 요청) | 1980년대 한국 집 느낌 소품: 앉은뱅이 밥상, 이불·요, 브라운관 TV, 자개장/옷장, 전화기, 달력·액자, 신문 더미, 쓰레기·박스, 냄비·연탄 등. **A가 무료 모델을 찾아 `Assets/Prefabs/Props/`에 프리팹으로 만들고(Collider 포함, 큰 가구는 NavMesh Modifier 고려), 배치는 B(SampleScene)**. 집마다 분위기 다르게: 201(평범·장롱), 301(일기장), **302(불 켜진 아이 방 — 장난감·작은 이불, 동생 흔적)**, 303(서랍 단서), 304(괴물 둥지 — 뜯긴 벽지·장판 더미), 관리사무소(책상·서류·열쇠함). 가구 놓은 뒤 NavMesh 다시 Bake |
| 14 | 열쇠 기능 (보일러실 열쇠) | 필수 | ⬜ | 지금 코드에 없음. `ItemPickup`에 Key 종류 + `DraggableDoor`에 필요한 열쇠 칸 → 없으면 "잠겨 있다", 있으면 열림. 체크포인트가 열쇠 기억. 302호에서 줍고 지하 문을 연다 (관리사무소 열쇠 꾸러미는 스토리용) |
| 15 | 1장 조작 안내 + 위층 소리 | 필수 | ⬜ (A가 할 예정, B와 확인) | Trigger 구역마다 OnGUI 안내(WASD·Shift·F·문 끌기·E). 1층에 있을 때 위층에서 쿵쿵·긁는 소리(괴물 소리 재사용) |
| 16 | 지하 최종 추격 + 체크포인트 4 + 탈출 → 엔딩 호출 | 필수 | ⬜ B 지하 맵 필요(11/20까지 요청) | `Monster_B1` 사본, 동생과 마주한 뒤 추격 시작, 비상구 Trigger → B 엔딩 |
| 17 | 밀린 테스트 | 필수 | ⬜ | ① 301호 들어가면 3층 괴물이 아래 계단에서 올라오는지 ② 점프스케어(얼굴이 화면 가운데, 비명) ③ "다시 시작/그만하기" 버튼 ④ 깜짝 포효 간격(테스트 땐 interval 20) ⑤ 2층 연출 처음부터 끝까지 ⑥ 3층에서 상자 던져 괴물 유인 → 끝나면 7~10번 상태 갱신 |
| 18 | (여유) 숨기 직전 소리 → 괴물이 그 장롱 확인 | 여유 | ⬜ | |
| — | **동결 후(11/23~30) A** | | ⬜ | 밸런스(건전지 수·위치, 괴물 속도, 배터리 소모) / NoiseSystem·MonsterAI Show Debug 끄기 / 안 쓰는 소리 변형 파일 정리(`_001~004`, `door_knock_b`, `door_break_b`, `breath_calm_b`, `breath_heavy_a` 등) / 상자 튕김 버그 / 강의 외 기능 최종 목록 / Credits 점검 |
| + | 괴물 발소리 + 2층용 기능 | 추가 | 🟡 스크립트 완료(11/17), A 테스트 전 | `MonsterAI`에 발소리(걸은 거리 1.1m마다 쿵, `Assets/Audio/Monster/monster_step_0~3` = Kenney 콘크리트 발소리를 낮고 무겁게 가공), **순찰 지점에서 멈추기**(`pausePointIndex`·`pauseTime`·`pauseLookAt`), **한 번만 지나가고 사라지기**(`patrolOnce`, `IsRouteFinished`). Monster 프리팹에 발소리 Audio Source(3D, Min 3, Max 40) 추가. 3층 괴물은 patrolOnce 끔·pause -1이라 예전과 같음 |
| + | 숨소리 | 추가 | 🟡 A 테스트 완료(11/15), B 확인 전 | `Player/PlayerBreath.cs`. Player 프리팹에 Loop Audio Source 2개(평소 `breath_calm_a`, 헐떡임 `breath_heavy_b`). 뛰면 숨참 0→1(5초), 쉬면 10초에 가라앉음, 두 소리를 섞음. 장롱 속에서 숨참 0.5 넘으면 1.5초마다 2.5m 소리(괴물이 확인하러 옴). 대체 후보 `breath_calm_b`, `breath_heavy_a` |

### B (월드·연출)
11/15 A가 정리. 순서대로 하면 A 기능과 바로 이어진다. 시작 전 `git pull`, SampleScene은 작게 나눠 자주 커밋.

**⚠️ (11/18) B 남은 일이 가장 많고 대부분 필수(MVP) — 없으면 게임이 시작·끝나지 않는다. 아래 마감 순서대로.**
- **11/19까지:** 0번 작은 수정 4개 + 3번 A 기능 확인
- **11/20까지:** 8번 지하 맵 (A 지하 추격이 이걸 기다림)
- **11/21까지:** 7번 바깥, 11번 스토리·단서, 12번 타이틀·엔딩
- **11/22:** 14번 꾸미기·재질, 13번 환경음 → 둘이 통플레이 → 기능 동결

| 순서 | 할 일 | 단계 | 상태 | 메모 |
|---|---|---|---|---|
| 0 | 바로 할 작은 수정 (아래 "알려진 문제") | 필수 | ⬜ | ① `Lintel_101~304`·`Lintel_Office` Scale Z 2 → 0.2 ② 계단실 맨 위(3층 → 지붕) 계단·경사로 지우기 ③ NavMesh 다시 Bake ④ `ClueReader`를 UIStyle로 (`UIStyle.Prompt`, `UIStyle.Body/Title`) ⑤ `Nightstand` 배치 |
| 1 | SampleScene NavMesh Bake | 필수 | ✅ A 확인(11/16) | 아래 배치 요청 표. 없으면 괴물이 못 움직임. 맵 바꿀 때마다 다시 Bake |
| 2 | A 프리팹 배치 | 필수 | ✅ A 확인(11/16) | 배치 요청 표 전부: Monster+순찰 지점, 장롱 3, 체크포인트 3, Breaker, Dresser·Nightstand·Box |
| 3 | A 기능 확인 → ✅ | 필수 | ⬜ | 장롱 숨기, 괴물(소리·추격·서성이기), 문 부수기, 사망·체크포인트, 숨소리, 서랍·레버 |
| 4 | UI 방식 A와 결정 | 필수 | ✅ A 확인(11/16): **OnGUI + 공통 스타일** | A 쪽 화면 표시(배터리·숨기·사망·줍기)가 전부 OnGUI라 맞춤. 단서 UI도 OnGUI로 작성함. 타이틀·엔딩·사망 선택 화면도 OnGUI로 (커서는 `Cursor.lockState = None`, `visible = true`로 보이기). A가 반대면 PROGRESS에 적어 주기 |
| 5 | 2층 연출 (셔터·센서등·괴물·"숨어라") | 필수 | 🟡 A가 수정(11/17), A 테스트 전 | `Story/Floor2Event.cs` (빈 오브젝트 `Floor2Event`). 레버 IsOn → 셔터가 0.4m 올라가다 걸림 → 센서등이 계단 쪽부터 하나씩 → 204호 앞에 괴물 → `FlickerOut` + 센서등 꺼짐 → "숨어라" → **(11/17 사용자 결정) 검은 실루엣 대신 진짜 Monster 프리팹 사본 `Monster_2F`** 의 AI가 켜져 순찰 지점을 **한 번만** 지나감: 202 앞 → 201 문 앞(밀어서 열고, 안 열리면 부숨) → **201호 안 장롱 앞에서 5초 킁킁**(Pause Point) → 다시 복도 → 계단에서 사라짐(`MonsterAI.IsRouteFinished`) → 셔터 끝까지. 진짜 괴물이라 소리 듣고, 숨지 않으면 쫓아와 잡음 → 체크포인트 2에서 다시. 셔터는 localScale.y를 줄여 위로 말려 올라감. 셔터·센서등 `Emit(…, 20)`. `Silhouette_2F`(검은 캡슐)와 Floor2Event의 실루엣 길·속도 설정은 안 씀(괴물 외형은 Monster 프리팹만 바꾸면 2·3층 같이 바뀜). **SampleScene은 A(사용자)가 직접 수정함(11/17) — B는 pull 후 작업**. **(11/18) "숨어라" 뒤에 잡히면** 체크포인트 2에서 다시 시작할 때 레버·센서등을 건너뛰고 셔터가 걸린 채로 괴물이 204호 앞에 서 있다가 1.5초 뒤 바로 "숨어라" + 출발(`Floor2Event.reachedHidePhase` static, `retryFromCheckpoint`·`retryDelay`). **(11/18) 2층 괴물은 201호 문을 밀지 않고 쾅쾅 부수고 들어옴**. **(11/18) 타이밍:** 마지막 센서등 0.3초 뒤 괴물 출발(`monsterStartDelay`) → 1.5초에 손전등 깜빡임(`flickerStartDelay`) → 불이 꺼지자마자 바로 "숨어라"(예전 `waitBeforeFlicker`·`waitBeforeCrawl`은 안 씀)(`Floor2Event.monsterBashesDoors` → `MonsterAI.alwaysBashDoors`, 3층 괴물은 예전처럼 순찰 중엔 밀어서 엶) |
| 6 | 단서 UI (일기장·사진·카세트) | 필수 | ✅ A 확인(11/16) | `Story/ClueItem.cs`(단서 물건: 종류·제목·pages·image·audioClip) + `UI/ClueReader.cs`(빈 오브젝트 `ClueReader` + Audio Source). 바라보면 "[E] 일기장 읽기", E → 누런 종이 화면, E로 다음 장/닫기. 읽는 동안 걷기·시점·잡기 멈춤. **Player 프리팹은 안 건드림**: E는 `Keyboard.current.eKey`로 직접 읽음(06장 Mouse.current 방식), Raycast는 Main Camera에서 |
| 7 | 단지 바깥 + 바깥에서 시작 | 필수 | ⬜ | 06장 Terrain 가능. Player와 체크포인트 1번을 바깥 시작점으로 옮기기 → NavMesh 다시 Bake |
| 8 | 지하 맵 (보일러실·제사상·비상구) | 필수 | ⬜ | 체크포인트 4번 + 괴물 최종 추격용 순찰 지점 (A에게 위치 알려 주기) |
| 9 | 3장 시작 시 Monster 켜기 | 필수 | 🟡 A가 수정(11/17), A 테스트 전 | `Story/ActivateOnEnter.cs`: Trigger 구역 `Chapter3Start`에 들어오면 Targets(Monster) SetActive(true). **(11/17 사용자 결정) 구역을 3층 계단 위 → 301호 안으로 옮김. 2층 괴물이 1층으로 내려간 뒤, 플레이어가 301호에 들어오면 3층 괴물이 2층 계단참(Enter Positions)에서 나타나 계단으로 올라와 복도를 지나 304호 쪽으로 감**(발소리가 아래에서 올라옴 → 301호 장롱에 숨기). 체크포인트 3에서 다시 시작하면 원래 자리(304호)에서 켬. FireShutter에 NavMesh Modifier(Ignore) + 다시 Bake (닫힌 셔터가 길을 끊지 않게) |
| 10 | 다시 시작 시 지난 연출 건너뛰기 | 목표 | ✅ A 확인(11/16) | `Floor2Event.skipFromCheckpoint`(3 이상이면 셔터 열림·실루엣 없음), `ActivateOnEnter.activeFromCheckpoint`(3 이상이면 Monster 바로 켬). 2층 연출 중 잡히면 체크포인트 2에서 연출 다시 |
| 11 | 스토리 연출·엔딩 | 필수 | ⬜ | ① **단서 내용**: 일기장 글(301), 사진 이미지·설명(302), 카세트테이프 소리/대사(302), 서랍 단서(303) — 반전(바쳐질 아이는 주인공, 대신 끌려간 건 동생)이 단계적으로 드러나게 ② **단서 배치** + 302호 **보일러실 열쇠**(A 14번 열쇠 기능 사용) ③ 302호 동생 흔적 ④ 지하 제사상(30년치) ⑤ **동생 모델(무료 리소스, Credits)** + 지하에서 마주하는 연출 → A 16번 추격 시작 신호 ⑥ 탈출 엔딩 연출 |
| 12 | 타이틀·엔딩 화면 | 필수 | ⬜ | OnGUI + UIStyle (A의 "다시 시작 / 그만하기"와 같은 방식). **게임은 SampleScene에서 시작해야 하므로 타이틀은 SampleScene 안에서 띄움**(시작 시 화면 위에 표시, 커서 보이기 → 시작 누르면 숨기고 게임 시작). 엔딩 화면 후 타이틀/종료 |
| 13 | 사운드 (배경음·환경음) | 목표 | ⬜ | 바람, 눈, 건물 삐걱임 등. 출처 Credits.txt |
| 14 | 방 안 꾸미기 배치 + 내부 재질 적용 | 목표 | ⬜ (11/18 추가) | A가 만드는 재질(A 11번)·소품 프리팹(A 13번)을 각 세대·관리사무소에 배치. 302호는 "아이 방"으로 가장 공들이기. 배치 후 NavMesh 다시 Bake. 단서(일기장·사진·카세트·**보일러실 열쇠**)도 이때 함께 |
| — | **11/22 기능 동결** | | | 이후 버그 수정·통플레이만 |
| — | **동결 후(11/23~30) B** | | ⬜ | 맵 구멍·끼임·빛 새는 곳 수정, 조명·안개 다듬기, 고칠 때마다 NavMesh Bake 확인 |

### 같이 (A + B)
| 날짜 | 할 일 | 상태 |
|---|---|---|
| 11/20 | B 지하 맵 → A 지하 추격 붙이기 | ⬜ |
| **11/22** | 처음(바깥)부터 엔딩까지 통플레이 1회 → **기능 동결** | ⬜ |
| 11/23~28 | 버그 수정 (고치면 여기에 기록) | ⬜ |
| 11/29 | 교차 테스트 (서로의 컴퓨터에서 처음부터 끝까지) | ⬜ |
| 11/30 | 발표 자료 + 시연 영상(약 5분: 2장 연출 + 3장 추격) → **밤에 미리 제출** | ⬜ |
| 12/01·12/08 | 발표 | ⬜ |

**발표 자료:** 컨셉·스토리, 맵 구조, 기능 ↔ 강의 대응표(CLAUDE.md 6장), 강의 외 기능 목록, 무료 리소스 출처, 시연 영상

**제출 체크리스트 (12/01 08:30, 팀 대표 1명)**
- [ ] Unity 6000.5.1f1 (`ProjectSettings/ProjectVersion.txt`)
- [ ] Build Profiles에서 SampleScene이 index 0
- [ ] SampleScene에 `Main Camera`(Player 자식, 이름·태그 유지)와 `Global Volume` 있음
- [ ] 직접 쓴 .cs는 전부 `Assets/MyScripts/`, 가져온 건 `Assets/OtherScripts/`(출처 주석)
- [ ] `Assets/Credits.txt`에 폰트·모델·Mixamo·소리·텍스처 전부
- [ ] 압축 시 `Library`·`Temp`·`Logs`·`obj` 제외 → 다른 폴더에 풀어서 열리는지 확인
- [ ] 발표 자료 함께 제출

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

- 11/17: **2층 괴물 = 진짜 Monster 프리팹 사본**(정해진 길을 한 번 지나가는 AI, 201호 안까지 들어와 장롱 앞 킁킁, 문 부수기 동일). 실루엣 전용 연출 코드는 없앰 (CLAUDE.md 4장 수정)
- 11/17: 2층 괴물은 계단 아래(1층)로 내려가 사라지고, 3층 괴물은 플레이어가 301호에 들어오면 아래 계단에서 올라온다 (`ActivateOnEnter.enterPositions`)

- 11/18: 남은 일 전체 정리. **역할 분담은 지금대로 유지** (A 시스템·소품 프리팹·재질 / B 맵·스토리·UI 화면·배치). B 마감 순서는 B 표 위 참고
- 11/18: 큐브 물건(Box·Breaker·Door·Dresser)은 프리팹 안에서 모델만 교체 → 씬 배치 그대로 유지

## 알려진 문제
- **(11/18, B에게) 계단실 맨 위 계단 없애기**: 3층에서 지붕(8.4) 쪽으로 올라가는 마지막 계단(Floor3/Stairwell의 Steps_B·Ramp_B 등)이 남아 있음 → 3층이 꼭대기라 위로 가는 계단은 지우거나 꺼 주기. 지운 뒤 NavMesh 다시 Bake
- (11/17) 문 위 벽(Lintel) 아래 NavMesh가 끊겨 괴물이 세대 안으로 못 들어가고 문 앞에서 멈춤 → **Navigation Agents > Humanoid Height 2 → 1.8**(괴물 키 1.9로 줄임과 함께) + 다시 Bake. 맵을 고치면 Bake 후 Scene 창 Show NavMesh로 문 앞이 이어졌는지 확인
- (11/17, B에게) 문 위 벽 `Lintel_101~304`, `Lintel_Office`의 Scale Z가 2라서 복도 쪽으로 0.9m 튀어나와 천장에 상자가 매달린 것처럼 보임 → **Scale Z를 0.2로** (Position z 2.1 그대로). 괴물이 여기에 파묻혀 보여서 A가 Monster 프리팹을 키 1.9m·폭 0.6m로 줄임(2층·3층 공통)
- (가끔) 상자를 들고 고개를 빠르게 돌린 뒤 놓으면 멀리 튕겨 나감 → 상자가 플레이어 몸을 통과하며 겹쳐서 생기는 현상. 다시 생기면 `Physics.IgnoreCollision` + `maxDepenetrationVelocity`(강의 외)로 고치기로 함

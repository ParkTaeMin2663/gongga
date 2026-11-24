# 진행 상황 — 「공가」

두 사람(A, B)과 각자의 Claude가 **같은 상황을 알기 위한 공유 기록**이다.
- 작업을 시작하기 전에 `git pull` → 이 파일을 읽는다.
- 작업을 끝내면 이 파일을 고치고 같이 커밋·푸시한다.
- 게임 설계·규칙이 바뀌면 이 파일이 아니라 **CLAUDE.md**를 고친다.

---

## 🔴 최종 일정 (11/23 A 정리) — 12/01(화) 08:30 제출 + 같은 날 발표
| 날짜 | 할 일 | 누가 |
|---|---|---|
| 11/23(월)~24(화) | **처음부터 엔딩까지 같이 통플레이** → 버그 목록 작성(아래 "알려진 문제"에) / 스토리 전달 결정(인트로 자막·기억 설정) | A·B |
| 11/25(수)~26(목) | 버그 수정, 난이도(괴물 속도·배터리·추격) 조절, 인트로 자막 넣기(정하면), **안 쓰는 파일 정리** | A·B |
| 11/27(금) | **시연 영상 녹화**(5분: 바깥 시작 → 2층 연출 → 3층 추격 → 엔딩 일부) + PPT용 스크린샷 | A·B |
| 11/28(토) | **PPT 초안** | A·B |
| 11/29(일) | **교차 테스트**(서로 컴퓨터에서 새로 clone → 처음부터 끝까지) + 발표 리허설 | A·B |
| 11/30(월) | 제출 직전 점검(아래 체크리스트) → **압축 → 다른 폴더에 풀어 Unity 6000.5.1f1로 열어 보기 → 밤에 미리 제출** | 대표 1명 |
| 12/01(화) | 08:30 제출 마감, 발표 | |

**제출 직전 점검 (11/30)**
- [ ] `Monster` 프리팹·`NoiseSystem` 프리팹·씬 `Monster`·`Monster_2F`·`Monster_B1`의 **Show Debug 끄기** (왼쪽 위 노란 "소리 #…" 글자)
- [ ] `TitleScreen`의 **Show Title 켜기**
- [ ] File > Build Profiles: **SampleScene이 index 0**
- [ ] SampleScene에 `Main Camera`(Player 자식, 이름·태그 유지), `Global Volume` 있음
- [ ] Console에 빨간 에러 없음 (노란 경고는 확인만)
- [ ] `Assets/Credits.txt` 빠진 리소스 없음, `강의외_기능_최종목록.md` 최신
- [ ] 압축에서 `Library`, `Temp`, `Logs`, `obj`, `UserSettings`, `.git` 제외 (`Assets`, `Packages`, `ProjectSettings`만) — **Git LFS 파일이 진짜 파일로 들어갔는지**(텍스처·소리 열어 보기) 확인
- [ ] 발표 PPT도 같이 제출

**발표 자료에 들어갈 것 (11/24 결정)**
- ✅ 넣는 것: ① **우리가 직접 구현한 기능** ② **자랑할 만한 기능**은 영상·캡처로 보여 주기 ③ **사용한 에셋 출처는 마지막 장**에 (AI로 만든 에셋은 "어떤 AI로 만들었다" 정도만 적기 — Credits.txt의 "팀 자체 제작 (Python으로…)" 항목 대부분은 Claude가 짠 Python으로 만든 것이라 PPT에는 "Claude(AI)로 생성한 코드로 제작"처럼 적기. A의 꽃무늬 벽지도 해당)
- ❌ 넣지 않는 것: 데모(시연), 역할 분담

## 지금 (11/15 ~ 11/22 기능 동결)
11/9 주 A 일이 밀려서 11/16 주 일과 합쳐 순서대로 진행한다. (11/15 결정)
B도 11/9 주 일(2층 연출·단서 UI·지하 맵·바깥 1차)을 못 해서 11/16 주에 합쳐 진행한다. (11/16)

### A (시스템)
| 순서 | 할 일 | 단계 | 상태 | 메모 |
|---|---|---|---|---|
| 1 | 장롱 숨기 | 목표 | ✅ B 확인(11/19) | `HideSpot.cs`(장롱) + `PlayerHide.cs`(Player). 장롱을 보고 **E 1초 꾹** → 문이 열리고 걸어 들어감 → 문 닫힘(12° 틈으로 엿봄, Peek Angle). E 한 번 → 나옴. 안에서는 이동·잡기 불가, 고개만. 모델 문짝을 DoorA/DoorB로 분리, 경첩은 문 모양에서 자동 계산. 모델 안 선반은 지움. 문 연결이 없으면 화면 어두워짐 방식으로 동작. 들어갈 때 3m 소리. 괴물은 `PlayerHide.IsHidden` 확인 (11/15: 문 닫히기 전, 발이 안에 들어간 순간부터 숨은 것으로 변경 — 들어가는 동안 잡히던 문제). 모델: Old Russian wardrobe 1983 (CC BY). 문 달린 모델이면 `doorLeft/Right` 연결 시 문이 실제로 열리는 방식도 지원. Interact 액션 = Press(Press And Release). Main Camera Near Clip 0.05 |
| 2 | 괴물 뼈대 + 소리 감지 | 필수 + 목표 | ✅ B 확인(11/19) | `Monster/MonsterAI.cs`, `Assets/Prefabs/Monster`(임시 빨간 캡슐 2.4m). 순찰(patrolPoints) → 소리 들으면 확인 → 2.5m 안 기척이면 추격(3.5m/s, 걷기 3 < 괴물 < 달리기 5.5) → 8m 밖이면 놓침 → 수색 4초 → 순찰. **장롱에 숨으면 그 장롱 앞을 8초 서성이며 가끔 쳐다봄(헐떡이면 시간 초기화) → 순찰**. 1.1m 안이면 잡힘 → 씬 재시작. 씬에 이름 `Player`·`NoiseSystem` 필요. 이동은 **NavMeshAgent**(07장)로 교체 완료(11/15). 다른 층 소리는 무시 |
| 3 | 괴물 이동 NavMeshAgent로 | 필수 | ✅ B 확인(11/19) | Monster 프리팹에 Nav Mesh Agent(Radius 0.3, Height 2.4). **Navigation 창 Agents > Humanoid Radius 0.3**(문 틈 1m를 지나가게, ProjectSettings에 저장) + NavMesh Modifier(Ignore From Build, 자기 자리 구멍 방지). Rigidbody는 Is Kinematic. Door 프리팹에도 NavMesh Modifier(Ignore From Build) → 닫힌 문도 길로 굽힘(문 앞 멈춤·부수기는 5번). **SampleScene에도 NavMesh Surface Bake 필요(B와 상의)** |
| 4 | 사망·체크포인트 | 목표 | ✅ B 확인(11/19) | `Core/Checkpoint.cs`(Is Trigger 구역, `public static` 저장값, order 큰 것만 저장) + `Player/PlayerDeath.cs`(잡히면 괴물 쪽으로 고개 → 붉게 → 까맣게(코루틴) → 씬 재로드 → 마지막 체크포인트 위치·방향에서 시작). Player 프리팹 Tag = `Player`. 위치만 되돌리고 물건·문 상태는 안 되돌림. **B 연출은 `Checkpoint.savedOrder`로 이미 지난 장면을 건너뛸 수 있음**. **나중에(11/22 전):** 사망 시 점프스케어(괴물 모델이 카메라로 덮침 + 비명, 7번 외형과 함께) + "다시 시작 / 그만하기" 선택 화면(B 타이틀 화면과 함께, UI 방식 결정·커서 보이기 필요). `PlayerDeath.DeathRoutine` 3단계(자동 재시작)를 바꾸면 됨 |
| 5 | 문 부수기 | 목표 | ✅ B 확인(11/19) | 괴물 앞 1.2m에 닫힌 문(Raycast) → 추격 중·잠긴 문이면 0.7초마다 쾅(3초) 후 **경첩 Destroy + 문짝 날림**(12m 소리), 평소엔 문을 밀어서 엶(안 열리면 부숨). `DraggableDoor`에 PushOpen/Knock/Break, 소리 `door_knock`(Basement door slam 한 방, 저음 강조판 `door_knock_b`), `door_break`(문 걷어차 부서지는 녹음 + 나무 박살, 대체 `door_break_b`)(자식 `BreakSoundPoint`에서 재생 → 부서질 때 떼어 놓아 그 자리에서 들림). 두 소리 Min Distance 6·도플러 0. 부서진 문짝이 떨어져 부딪히면 `door_fall` 쿵(OnCollisionEnter, 세기에 따라 볼륨). 부서진 문은 못 잡음. 문 틈(1.0m)에 덜 걸리게 **Player Capsule Radius 0.4 → 0.3**(11/15) |
| 6 | 손전등 배터리 | 목표 | ✅ B 확인(11/19) | `PlayerFlashlight.cs`: 켜면 0.5/초 소모(100 → 3분 20초), 20% 아래 어두워지며 깜빡, 0이면 안 켜짐. 왼쪽 아래 배터리 막대(OnGUI). 딸깍 소리. **B 연출용 `FlickerOut(초)`** (깜빡이다 꺼짐, 코루틴), `GiveFlashlight()`(관리사무소에서 얻을 때, `hasFlashlight` 끄고 시작). 건전지 `Assets/Prefabs/Battery`(+50). **설계대로(11/15): Player는 손전등 없이 시작(`hasFlashlight` 끔)** → 관리사무소 `FlashlightPickup`을 **E로 주워** 얻음(Poly Haven Plastic Torch 6V). 줍는 물건은 `ItemPickup.cs`(종류: Flashlight/Battery) + Player의 `PlayerPickup.cs`(바라보면 "[E] 손전등 줍기"). **얻으면 왼손에 들림**(Main Camera > `HeldFlashlight`, 불빛이 렌즈에서 나옴, 걸으면 `FlashlightSway`로 살짝 흔들림·뛰면 크게). 건전지 모델 D Battery(CC BY). 체크포인트가 손전등 보유·배터리 양도 기억 → 죽어도 안 잃음 |
| 7 | 괴물 외형 (무료 모델) | 여유 | 🟡 모델·동작 넣음(11/18), A 테스트 전 | **Cursed Rune Demon**(Sketchfab, CC BY, 부적 글씨가 새겨진 괴물 — "뒤틀린 집 지킴이 신") 결정. 뼈대가 Mixamo 형식이라 **Humanoid** + Mixamo 동작(Walk·Run·LookAround·BashDoor·Scream) + 모델 자체 Idle. `Assets/Models/Monster/`(모델·동작·`MonsterAnimator.controller`). Monster 프리팹에 자식 `Model`(1.05배, 키 약 1.9m)로 들어가고 빨간 캡슐(Body·Face_Front)은 꺼 둠(충돌·길찾기는 그대로). `MonsterAI`가 속도·상태로 동작 고름([강의 외] Animator.CrossFade, 11/18 허락). 2층·3층 괴물 공통. 다음: 사망 점프스케어 |
| 8 | 공통 UI 스타일 | 필수 | 🟡 스크립트 완료(11/16), A 테스트 전 | `UI/UIStyle.cs` — **Player 프리팹에 붙어 있음**(글꼴 연결됨). 글꼴 **Noto Serif KR(본명조) Regular/Bold/Black**(`Assets/Fonts`, OFL, Credits 기록) 하나로 통일. 색: `UIStyle.white/yellow/red/paper/gray`. 쓰는 법(OnGUI 안): `UIStyle.Prompt("[E] 일기장 읽기")`(조준점 아래, [E] 노란색), `UIStyle.Label(rect, 글자, UIStyle.Title(40))`(그림자 있는 글자), 스타일만: `UIStyle.Body(22, UIStyle.paper)` / `Strong` / `Title`. 크기는 1080p 기준 숫자를 넣으면 화면 높이에 맞춰 바뀜. A 쪽 적용 완료: 줍기·숨기 안내, E 막대, "저장됨". **B: `ClueReader`(안내 문구·종이 글자)를 UIStyle로 바꿔 주기.** `Floor2Event`의 "숨어라"는 A가 UIStyle로 바꿈(11/16, 크기 36 → `hideHintSize` 120, 본명조 Black·그림자, 사용자 요청), 타이틀·엔딩도 UIStyle 사용. 제출 전 `NoiseSystem`·`MonsterAI`의 Show Debug 끄기 |
| 9 | 사망 점프스케어 + "다시 시작/그만하기" | 목표 | 🟡 스크립트 완료(11/18), A 테스트 전 | `PlayerDeath`: 잡히면 괴물 쪽으로 0.15초에 고개 → **괴물이 얼굴 앞 0.55m로 덮침**(NavMeshAgent 잠깐 끔, MonsterAI가 Scream 동작) + 화면 흔들림 1초 → 뚝 까매짐 → **"잡혔다" + 다시 시작 / 그만하기**(UIStyle, 마우스 커서 보임, 올리면 노란색). 다시 시작 = 체크포인트, 그만하기 = 종료(에디터는 Play 멈춤). 비명 소리 칸 `jumpscareSound`는 10번에서 채움. Use Jumpscare 끄면 예전 방식. 2층 괴물은 레버를 올리는 순간부터 복도 끝에 서 있음(11/18) |
| 10 | 배경음·긴장 음악 + 괴물 소리 | 목표 | 🟡 3차 수정(11/18), A 테스트 전 | ① **`Core/TensionMusic.cs`**(Player 프리팹 + 2D Loop Audio Source 2개): **배경음 `ambient_loop`(게임 시작부터 계속, 0.35)**, **"숨어라" = 긴박한 음악 + 심장 소리를 섞은 `hide_heartbeat_loop`(1분 42초, 11/18 교체)**(Floor2Event `PlayHide/StopHide`), **추격 = `chase_loop`**(MonsterAI `PlayChase/StopChase`, 우선). 긴장 음악이 나오면 배경음은 0.08로 줄어듦. 서서히 켜짐 1.2초·꺼짐 4초. B: 배경음을 바꾸려면 Player 프리팹 TensionMusic의 Ambient Clip ② **괴물 소리**(Monster 프리팹 `voiceSource`, 3D Loop): 숨소리 `monster_breath_loop`(0.7), **킁킁 `monster_sniff_loop`(1.0, 무음 구간 잘라냄, 처음부터 재생 — 장롱 앞에서 안 들리던 문제)**, 추격 포효 `monster_roar`, 문 부수기 으르렁 `monster_growl`, 점프스케어 비명 `monster_scream`. 모두 Pixabay(Credits). ③ **`Core/RoarScare.cs`**(Player 프리팹): "숨어라" 순간 괴물 쪽에서 포효 → 그 뒤 **약 3분(180±40초)마다 깜짝 포효**(괴물 방향, 없으면 아래층 어딘가, 플레이어 7m 안으로 당겨 재생, 추격 중엔 건너뜀, 다시 시작해도 계속) `monster_roar`·`monster_roar_far` |
| 11 | 아파트 내부 재질 (벽지·장판·천장·문·선반) | 목표 | 🟡 재질 완료(11/20), 씬 적용 전 | **Poly Haven(CC0) 2K 텍스처 → `Assets/Materials/Interior/`**: `Interior_Wallpaper`(낡은 벽지)·`_Yellow`, `Interior_Jangpan`(노란 장판)·`Interior_Linoleum`, `Interior_Ceiling`, `Corridor_Floor`(닳은 콘크리트), `Corridor_Railing`(페인트 벗겨진 벽), `Door_SteelPaint`(녹슨 페인트 철문), `Office_FloorTiles`, `Basement_Wall`, `Basement_Floor`. **`Unit` 프리팹(201·301~303)에 적용함**: 벽 5개 = **꽃무늬 벽지 `Interior_Wallpaper_Floral`(A 직접 제작, 11/20 — 흰 벽지가 천장과 구분이 안 돼서 교체)**, 천장은 차가운 회색으로, 새 자식 `Floor_Jangpan`(바닥 위 0.005 Quad)·`Ceiling_Room`(2.59 아래 보는 Quad) — 바닥·천장이 층 판(Slab) 하나라 따로 덧댐. **`Door` 프리팹 = Door_SteelPaint(모든 문)**. 씬에 직접 칠할 곳(Slab_Main·Railing·복도 Ceiling·304호·관리사무소·지하)은 아래 "재질 적용 안내" |
| 12 | 기본 큐브 물건 → 진짜 모델로 교체 | 목표 | 🟡 Box 교체(11/20), A 테스트 전 | **`Box` = 큐브 렌더러 끄고 자식 `Model`(cardboard_box_01, 실제 크기 0.39×0.52×0.34) + 콜라이더 맞춤**. `Breaker` = (11/22) A의 `Model_PowerBox`는 B의 `breaker_plate`와 겹쳐서 **뺌** → B 모델(이름판·레버)만 사용. `Door` = 재질 교체. ~~`Dresser`(303 서랍장 큐브)~~ → **B가 `Nightstand_303`(-4, 5.6, 7.9)으로 교체(11/22)**: `Clue_Note_303`을 서랍 자식으로 옮김, `Dresser`는 꺼 둠(지우지 않음), `Battery_303` (-4.1, 6.25, 7.85). **(11/22 B) `Door` 프리팹 = 직접 만든 철문 모델 `door_steel`로 교체, `Breaker` = 레버 손잡이 `breaker_lever`·이름판 `breaker_plate` 추가 (큐브 렌더러만 끔, 스크립트·콜라이더 그대로) → A는 통플레이 때 문 끌기·부수기·레버 동작만 확인** |
| 13 | 방 안 꾸미기용 소품 프리팹 | 목표 | 🟡 프리팹 22개 완료(11/20), 배치 전 | `Assets/Prefabs/Props/Prop_*.prefab` (Box Collider = 모델 실제 크기, 자식에 모델): CardboardBox, PowerBox, OfficeDesk, Chair, SteelShelves, DrawerCabinet, Binder, Clipboard, WallClock, TrashCan, TV(브라운관), BedFrame, Cabinet, Bookshelf, AlarmClock, Pot, Suitcase, TrashBag, LightBulbSocket, CassettePlayer, RubberDuck, Payphone(한국 공중전화). 모델 `Assets/Models/Props/PolyHaven/`, 재질 `Assets/Materials/Props/PolyHaven/`(FBX 재질 자동 연결). 큰 가구 배치 후 NavMesh 다시 Bake. 벽시계·전구는 원점이 가운데일 수 있어 콜라이더 위치 확인 |
| 14 | 열쇠 기능 (보일러실 열쇠) | 필수 | 🟡 스크립트 완료(11/20), A 테스트 전 | **(11/20)** `Player/PlayerKeys.cs`(Player 프리팹, static 열쇠 목록 + 화면 아래 안내 문구 "~를 얻었다"/"잠겨 있다") + `ItemPickup` 종류 **Key**(Key Name) + `DraggableDoor`의 **Required Key**·Unlock Clip(맞는 열쇠가 있으면 잡는 순간 풀림) + Checkpoint가 열쇠 기억(다시 시작 시 이미 가진 열쇠 오브젝트는 사라짐). 씬 설정: `Key_Boiler`에 ItemPickup(Key, "보일러실 열쇠", metalClick), `Door_Basement`에 Is Locked 체크 + Required Key "보일러실 열쇠" + metalLatch (Door_Basement는 원래 잠금이 꺼져 있었음). 예전 메모: 지금 코드에 없음. `ItemPickup`에 Key 종류 + `DraggableDoor`에 필요한 열쇠 칸 → 없으면 "잠겨 있다", 있으면 열림. 체크포인트가 열쇠 기억. 302호에서 줍고 지하 문을 연다 (관리사무소 열쇠 꾸러미는 스토리용) |
| 15 | 1장 조작 안내 + 위층 소리 | 필수 | 🟡 **B가 만들고 배치(11/22)**, A 확인 전: `UI/TutorialHint.cs`(Trigger 구역 안내 4개 `Hints/Hint_Move·Office·Door·Item`, 한 번만·타이틀 뒤·체크포인트 2 이후 안 뜸) + `Story/UpstairsNoise.cs`(오브젝트 `UpstairsNoise`, 1층에 있는 동안 10~20초마다 위층 괴물 발소리·으르렁) | Trigger 구역마다 OnGUI 안내(WASD·Shift·F·문 끌기·E). 1층에 있을 때 위층에서 쿵쿵·긁는 소리(괴물 소리 재사용) |
| 16 | 지하 최종 추격 + 체크포인트 4 + 탈출 → 엔딩 호출 | 필수 | 🟡 **연결 완료, 통플레이 확인만 남음(11/22 B 확인)**: B의 `BrotherEncounter`가 연출 끝에 `Monster_B1`(계단 쪽 17.5, -2.8, 1)을 켜고 0.5초 뒤 플레이어 자리에서 40m 소리 → 괴물이 확인하러 옴(2.2m/s), 달리거나 2.5m 안이면 추격(3.5m/s). 순찰 B1_Patrol 4개, Checkpoint_4(재시작 시 2초 뒤 바로 추격), ExitTrigger → EndingScreen. **통플레이에서 볼 것**: 너무 쉬우면 Investigate Speed·noiseLoudness 올리기, 너무 어려우면 Monster_B1 시작 위치를 보일러 쪽으로 | `Monster_B1` 사본, 동생과 마주한 뒤 추격 시작, 비상구 Trigger → B 엔딩 |
| 17 | 밀린 테스트 | 필수 | ⬜ | ① 301호 들어가면 3층 괴물이 아래 계단에서 올라오는지 ② 점프스케어(얼굴이 화면 가운데, 비명) ③ "다시 시작/그만하기" 버튼 ④ 깜짝 포효 간격(테스트 땐 interval 20) ⑤ 2층 연출 처음부터 끝까지 ⑥ 3층에서 상자 던져 괴물 유인 → 끝나면 7~10번 상태 갱신 |
| 18 | (여유) 숨기 직전 소리 → 괴물이 그 장롱 확인 | 여유 | ⬜ | |
| — | **동결 후(11/23~30) A** | | ⬜ | 밸런스(건전지 수·위치, 괴물 속도, 배터리 소모) / NoiseSystem·MonsterAI Show Debug 끄기 / 안 쓰는 소리 변형 파일 정리(`_001~004`, `door_knock_b`, `door_break_b`, `breath_calm_b`, `breath_heavy_a` 등) / 상자 튕김 버그 / 강의 외 기능 최종 목록 / Credits 점검 |
| + | 괴물 발소리 + 2층용 기능 | 추가 | 🟡 스크립트 완료(11/17), A 테스트 전 | `MonsterAI`에 발소리(걸은 거리 1.1m마다 쿵, `Assets/Audio/Monster/monster_step_0~3` = Kenney 콘크리트 발소리를 낮고 무겁게 가공), **순찰 지점에서 멈추기**(`pausePointIndex`·`pauseTime`·`pauseLookAt`), **한 번만 지나가고 사라지기**(`patrolOnce`, `IsRouteFinished`). Monster 프리팹에 발소리 Audio Source(3D, Min 3, Max 40) 추가. 3층 괴물은 patrolOnce 끔·pause -1이라 예전과 같음 |
| + | 숨소리 | 추가 | ✅ B 확인(11/19) | `Player/PlayerBreath.cs`. Player 프리팹에 Loop Audio Source 2개(평소 `breath_calm_a`, 헐떡임 `breath_heavy_b`). 뛰면 숨참 0→1(5초), 쉬면 10초에 가라앉음, 두 소리를 섞음. 장롱 속에서 숨참 0.5 넘으면 1.5초마다 2.5m 소리(괴물이 확인하러 옴). 대체 후보 `breath_calm_b`, `breath_heavy_a` |

### B (월드·연출)
11/15 A가 정리. 순서대로 하면 A 기능과 바로 이어진다. 시작 전 `git pull`, SampleScene은 작게 나눠 자주 커밋.

**⚠️ (11/18) B 남은 일이 가장 많고 대부분 필수(MVP) — 없으면 게임이 시작·끝나지 않는다. 아래 마감 순서대로.**
- **11/19까지:** 0번 작은 수정 4개 + 3번 A 기능 확인
- **11/20까지:** 8번 지하 맵 (A 지하 추격이 이걸 기다림)
- **11/21까지:** 7번 바깥, 11번 스토리·단서, 12번 타이틀·엔딩
- **11/22:** 14번 꾸미기·재질, 13번 환경음 → 둘이 통플레이 → 기능 동결

| 순서 | 할 일 | 단계 | 상태 | 메모 |
|---|---|---|---|---|
| 0 | 바로 할 작은 수정 (아래 "알려진 문제") | 필수 | ✅ B(11/20) | ① `Lintel_101~304`·`Lintel_Office` Scale Z 2 → 0.2 ② 계단실 맨 위(3층 → 지붕) 계단·경사로 지우기 ③ NavMesh 다시 Bake ④ ~~`ClueReader`를 UIStyle로~~ 코드 완료(11/19, 안내 = `UIStyle.Prompt`, 제목 = `Title`, 본문 = `Body`, Inspector Font Size 26) ⑤ `Nightstand` 배치 |
| 1 | SampleScene NavMesh Bake | 필수 | ✅ A 확인(11/16) | 아래 배치 요청 표. 없으면 괴물이 못 움직임. 맵 바꿀 때마다 다시 Bake |
| 2 | A 프리팹 배치 | 필수 | ✅ A 확인(11/16) | 배치 요청 표 전부: Monster+순찰 지점, 장롱 3, 체크포인트 3, Breaker, Dresser·Nightstand·Box |
| 3 | A 기능 확인 → ✅ | 필수 | ✅ B 확인(11/19) | 장롱 숨기, 괴물(소리·추격·서성이기), 문 부수기, 사망·체크포인트, 숨소리, 서랍·레버 |
| 4 | UI 방식 A와 결정 | 필수 | ✅ A 확인(11/16): **OnGUI + 공통 스타일** | A 쪽 화면 표시(배터리·숨기·사망·줍기)가 전부 OnGUI라 맞춤. 단서 UI도 OnGUI로 작성함. 타이틀·엔딩·사망 선택 화면도 OnGUI로 (커서는 `Cursor.lockState = None`, `visible = true`로 보이기). A가 반대면 PROGRESS에 적어 주기 |
| 5 | 2층 연출 (셔터·센서등·괴물·"숨어라") | 필수 | 🟡 A가 수정(11/17), A 테스트 전 | `Story/Floor2Event.cs` (빈 오브젝트 `Floor2Event`). 레버 IsOn → 셔터가 0.4m 올라가다 걸림 → 센서등이 계단 쪽부터 하나씩 → 204호 앞에 괴물 → `FlickerOut` + 센서등 꺼짐 → "숨어라" → **(11/17 사용자 결정) 검은 실루엣 대신 진짜 Monster 프리팹 사본 `Monster_2F`** 의 AI가 켜져 순찰 지점을 **한 번만** 지나감: 202 앞 → 201 문 앞(밀어서 열고, 안 열리면 부숨) → **201호 안 장롱 앞에서 5초 킁킁**(Pause Point) → 다시 복도 → 계단에서 사라짐(`MonsterAI.IsRouteFinished`) → 셔터 끝까지. 진짜 괴물이라 소리 듣고, 숨지 않으면 쫓아와 잡음 → 체크포인트 2에서 다시. 셔터는 localScale.y를 줄여 위로 말려 올라감. 셔터·센서등 `Emit(…, 20)`. `Silhouette_2F`(검은 캡슐)와 Floor2Event의 실루엣 길·속도 설정은 안 씀(괴물 외형은 Monster 프리팹만 바꾸면 2·3층 같이 바뀜). **SampleScene은 A(사용자)가 직접 수정함(11/17) — B는 pull 후 작업**. **(11/18) "숨어라" 뒤에 잡히면** 체크포인트 2에서 다시 시작할 때 레버·센서등을 건너뛰고 셔터가 걸린 채로 괴물이 204호 앞에 서 있다가 1.5초 뒤 바로 "숨어라" + 출발(`Floor2Event.reachedHidePhase` static, `retryFromCheckpoint`·`retryDelay`). **(11/18) 2층 괴물은 201호 문을 밀지 않고 쾅쾅 부수고 들어옴**. **(11/18) 타이밍:** 마지막 센서등 0.3초 뒤 괴물 출발(`monsterStartDelay`) → 1.5초에 손전등 깜빡임(`flickerStartDelay`) → 불이 꺼지자마자 바로 "숨어라"(예전 `waitBeforeFlicker`·`waitBeforeCrawl`은 안 씀)(`Floor2Event.monsterBashesDoors` → `MonsterAI.alwaysBashDoors`, 3층 괴물은 예전처럼 순찰 중엔 밀어서 엶) |
| 6 | 단서 UI (일기장·사진·카세트) | 필수 | ✅ A 확인(11/16) | `Story/ClueItem.cs`(단서 물건: 종류·제목·pages·image·audioClip) + `UI/ClueReader.cs`(빈 오브젝트 `ClueReader` + Audio Source). 바라보면 "[E] 일기장 읽기", E → 누런 종이 화면, E로 다음 장/닫기. 읽는 동안 걷기·시점·잡기 멈춤. **Player 프리팹은 안 건드림**: E는 `Keyboard.current.eKey`로 직접 읽음(06장 Mouse.current 방식), Raycast는 Main Camera에서 |
| 7 | 단지 바깥 + 바깥에서 시작 | 필수 | ✅ B 배치(11/20): 펜스·낙서, 철문·경비실, 주차장·차 3대, 화단·나무, 놀이터, 가로등 5(2개 꺼짐), 눈 파티클·안개, 눈 발소리 구역(`FootstepZone`). 모델·텍스처 모두 자체 제작 | `floor` Plane을 마당(z -30~0)만 덮게 줄임(지하 계단 구멍 때문). 펜스 경계 x ±30·z -30, 입구·경비실(남), 주차장(서), 놀이터(동남), 화단·앙상한 나무, 가로등 일부만 켬, 눈 Particle System(06·07장), Fog. **Player·Checkpoint_1 → (2, 0, -27) 북쪽을 봄**(정면 위에 302호 불빛) → NavMesh 다시 Bake |
| 8 | 지하 맵 (보일러실·제사상·비상구) | 필수 | ✅ B 배치(11/20): 보일러·제사상(제기·축문)·동생 모델·비상구·Checkpoint_4·Monster_B1·NavMesh Bake | 빈 오브젝트 `B1`(y -2.8)에 1층 `Stairwell`·`Slab` 복사 → 1층 계단실 x 19 쪽에서 내려가는 계단. 1층 계단실 `Door_Basement`(19.05, 1.05, 2.0) = **보일러실 열쇠 문(A 14)**. 지하 = x -16~16 한 칸, 가운데 보일러 4개(x -8/-3/2/7, z 4)를 사이에 두고 남·북 두 길로 빙 돌 수 있음. 서쪽 끝 제사상(-15.2, -2.8, 4)·동생, 북동쪽 비상구 `Door_Exit`(13, -1.75, 8.5) → `ExitTrigger`. **A 16번용 위치는 아래 "B → A 알림"** |
| 9 | 3장 시작 시 Monster 켜기 | 필수 | 🟡 A가 수정(11/17), A 테스트 전 | `Story/ActivateOnEnter.cs`: Trigger 구역 `Chapter3Start`에 들어오면 Targets(Monster) SetActive(true). **(11/17 사용자 결정) 구역을 3층 계단 위 → 301호 안으로 옮김. 2층 괴물이 1층으로 내려간 뒤, 플레이어가 301호에 들어오면 3층 괴물이 2층 계단참(Enter Positions)에서 나타나 계단으로 올라와 복도를 지나 304호 쪽으로 감**(발소리가 아래에서 올라옴 → 301호 장롱에 숨기). 체크포인트 3에서 다시 시작하면 원래 자리(304호)에서 켬. FireShutter에 NavMesh Modifier(Ignore) + 다시 Bake (닫힌 셔터가 길을 끊지 않게) |
| 10 | 다시 시작 시 지난 연출 건너뛰기 | 목표 | ✅ A 확인(11/16) | `Floor2Event.skipFromCheckpoint`(3 이상이면 셔터 열림·실루엣 없음), `ActivateOnEnter.activeFromCheckpoint`(3 이상이면 Monster 바로 켬). 2층 연출 중 잡히면 체크포인트 2에서 연출 다시 |
| 11 | 스토리 연출·엔딩 | 필수 | ✅ B 배치(11/20): BrotherEncounter·ExitTrigger(EndingScreen)·302호 동생 흔적(신발·이부자리·장난감·크레파스 그림 단서·보일러실 열쇠 자리). 통플레이 확인 필요 | `Story/BrotherEncounter.cs`(제사상 앞 Trigger → 고개 돌아감 → 동생 자막 → 동생 사라짐·촛불 꺼짐 → "도망쳐" → `activateAfter`(Monster_B1) 켜고 0.5초 뒤 플레이어 자리 `Emit(40m)`; 체크포인트 4에서 다시 시작하면 자막 없이 2초 뒤 추격, `BrotherEncounter.finished` static), `UI/EndingScreen.cs`(비상구 Trigger → 괴물 끄기·까매짐·소리 줄임 → 엔딩 글 → "처음으로/그만하기"). 제사상 축문 단서, 302호 크레파스 그림·신발. | ① **단서 내용**: 일기장 글(301), 사진 이미지·설명(302), 카세트테이프 소리/대사(302), 서랍 단서(303) — 반전(바쳐질 아이는 주인공, 대신 끌려간 건 동생)이 단계적으로 드러나게 ② **단서 배치** + 302호 **보일러실 열쇠**(A 14번 열쇠 기능 사용) ③ 302호 동생 흔적 ④ 지하 제사상(30년치) ⑤ **동생 모델(무료 리소스, Credits)** + 지하에서 마주하는 연출 → A 16번 추격 시작 신호 ⑥ 탈출 엔딩 연출 |
| 12 | 타이틀·엔딩 화면 | 필수 | ✅ B 배치(11/20): TitleScreen·EndingScreen(엔딩 피아노). 제출 전 Show Title 켜져 있는지 확인 | `UI/TitleScreen.cs`(빈 오브젝트 `TitleScreen`. 어두운 막 + "공가/空家" + 시작하기·그만하기, 커서 보임·플레이어 멈춤, `hasStarted` static → 죽어서 다시 시작하면 안 뜸, 테스트용 `Show Title` 끄기). 엔딩 "처음으로" = 체크포인트·연출 static 전부 지우고 씬 재로드 → 타이틀부터. | OnGUI + UIStyle (A의 "다시 시작 / 그만하기"와 같은 방식). **게임은 SampleScene에서 시작해야 하므로 타이틀은 SampleScene 안에서 띄움**(시작 시 화면 위에 표시, 커서 보이기 → 시작 누르면 숨기고 게임 시작). 엔딩 화면 후 타이틀/종료 |
| 13 | 사운드 (배경음·환경음) | 목표 | ✅ B(11/20): 환경음 Amb_Wind·Amb_Boiler·Amb_Creak, 셔터 소리 교체, 엔딩 피아노 (자체 합성) | `Assets/Audio/Ambience/`: 바깥 바람(3D, Max 45), 지하 보일러 웅웅(3D, Max 20), 건물 삐걱임, (없어도 됨) 동생 목소리·엔딩 음악·셔터 소리 교체. Pixabay, 출처 Credits.txt |
| 14 | 방 안 꾸미기 배치 + 내부 재질 적용 | 목표 | ✅ B(11/21): 302호 자체 소품 + A가 301·303·304·201호·관리사무소 배치 + B 204호(의자·봉투·상자·벽시계) + 남은 재질(3층 복도 바닥, 304·204호 벽, 304호 장판, 지하 벽·바닥) | A가 만드는 재질(A 11번)·소품 프리팹(A 13번)을 각 세대·관리사무소에 배치. 302호는 "아이 방"으로 가장 공들이기. 배치 후 NavMesh 다시 Bake. 단서(일기장·사진·카세트·**보일러실 열쇠**)도 이때 함께 |
| 15 | 퀄리티: 큐브·빈 곳 → 직접 만든 모델 | 여유 | ✅ B(11/22 새벽), 통플레이 확인 필요 | 철문, 차단기 레버·이름판, 단서 4개, 창문·방범창, 지하 보일러·제사상·촛대·쌀가마니·배관, 계단 손잡이, 비상구 표지, 트럭 보강, 방화 셔터, 천장 등 3종, 모서리 때, Lintel 벽지, 스프레이 재질 수정. 자세한 건 아래 "재질 적용 안내" 밑 (11/22 새벽 B) 메모 |
| — | **11/22 기능 동결** | | | 이후 버그 수정·통플레이만 |
| — | **동결 후(11/23~30) B** | | ⬜ | 맵 구멍·끼임·빛 새는 곳 수정, 조명·안개 다듬기, 고칠 때마다 NavMesh Bake 확인 |

### 같이 (A + B)
| 날짜 | 할 일 | 상태 |
|---|---|---|
| 11/20 | B 지하 맵 → A 지하 추격 붙이기 | ✅ 지하 맵·추격 연결 끝 (B 11/20~22), 통플레이로 확인 |
| **11/22** | 처음(바깥)부터 엔딩까지 통플레이 1회 → **기능 동결** | ⬜ A·B 만나서 같이 (A 17번 밀린 테스트 + 🟡 항목 확인을 이때 한 번에) |
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
- [ ] `TitleScreen`의 **Show Title 체크 켜짐** (테스트하느라 껐으면)
- [ ] `Monster`·`NoiseSystem` 프리팹과 씬의 `Monster`·`Monster_2F`·`Monster_B1`에서 **Show Debug 꺼짐** (화면 왼쪽 위 빨간 글자가 안 보이는지)

---

## 지난주 (11/2 ~ 11/8)

### A (시스템)
| 할 일 | 상태 | 메모 |
|---|---|---|
| ① 물건 집기 | ✅ B 확인(11/03) | `PickupObject.cs`, `Assets/Prefabs/Box`. Test_A 씬 |
| ② 물건 던지기 | ✅ B 확인(11/03) | 들고 있을 때 마우스 오른쪽. 새 액션 `Throw` (InputSystem_Actions) |
| ③ 서랍 | ✅ B 확인(11/19) | `DraggableDrawer.cs`, `Assets/Prefabs/Dresser`, `Assets/Prefabs/Nightstand`(무료 모델). 가져온 모델은 서랍 부품에 Box Collider + Rigidbody + DraggableDrawer, 방향은 Open Direction으로 맞춤. Dresser(Empty, Scale 1, +Z가 앞) 아래 Body + Drawer(Rigidbody Use Gravity 끔·Is Kinematic). 당기면 열림, 최대 0.35m |
| ④ 차단기 레버 | ✅ B 확인(11/19) | `DraggableLever.cs`. `Assets/Prefabs/Breaker`. Breaker(Empty) 아래 Box + Lever(Empty, 회전축) + 자식 Handle(Cube). 재질은 기존 door_material(몸통)·maker material(손잡이). 마우스 위로 밀면 올라감, 끝까지 올리면 `IsOn = true`(한 번 켜지면 고정). B 셔터 연출은 `lever.IsOn` 확인 |
| ⑤ NoiseSystem 뼈대 | ✅ B 확인(11/19) | `Noise/NoiseSystem.cs`, 씬에 빈 오브젝트 `NoiseSystem`(이름 정확히) 1개. 소리 내기: Start()에서 `GameObject.Find("NoiseSystem")` → `Emit(위치, 크기m, 이름)`. 연결됨: 달리기 6m(걸음마다 0.35초, 걷기는 실제 발소리만), 물건 부딪힘 8m(2m/s 이상), 차단기 15m. 괴물은 `NoiseCount` 변화 + `CanHear(위치)` + `LastPosition` 사용 예정. B 셔터·센서등은 20m 권장. 실제 발소리: Player의 Audio Source + `Assets/Audio/Footsteps`(Kenney CC0), 걷기 작게·달리기 크게. 효과음(11/07): Box 부딪힘, 레버 철컥, 서랍·문 열기/닫기/잠김 — 프리팹에 Audio Source로 들어 있음(Spatial Blend 3D). `DraggableDrawer`·`DraggableDoor`에 Open/Close/Locked Sound 칸 추가. 소리 파일 `Assets/Audio/`(Kenney CC0, Pixabay), Credits 기록. 안 쓰는 변형 파일(_001~004 등)은 나중에 정리 |

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
- 11/19: 지하 = 1층 계단실 열쇠 문 → 보일러실 한 칸(보일러 줄을 사이에 둔 두 길) → 서쪽 제사상·동생 → 북동쪽 비상구. 바깥 = 건물 남쪽 마당(x ±30, z -30~0), 남쪽 입구에서 시작. 타이틀은 SampleScene 위에 OnGUI로 그리고, 죽어서 다시 시작하면 안 뜸. 엔딩 "처음으로" → 타이틀

## B → A 알림 (11/19, 지하 좌표 — A 16번용)
- **(11/22 B) A에게 알림** — ① **15번은 B가 만들고 배치함** (`UI/TutorialHint.cs` 안내 4개 + `Story/UpstairsNoise.cs` 위층 소리) → A는 같은 걸 만들지 말고 확인만 ② **16번은 BrotherEncounter로 이미 연결됨** → 통플레이 때 추격 난이도만 같이 보기 ③ 303호 `Dresser` → `Nightstand_303` 교체(A 12번 남은 것) ④ 그림자 경고("Reduced additional punctual light shadows…") 원인 = 그림자 켜진 Point Light 4개(`Corridor_Light` ×3, `Guard_Light`, 하나당 6장) → B가 No Shadows로, Shadow Atlas 4096 ⑤ **11/30 제출 직전 `Monster`·`NoiseSystem` 프리팹과 씬 Monster 3개의 Show Debug 끄기** (왼쪽 위 빨간 테스트 글자) ⑥ 강의 외 기능 최종 목록은 B가 정리함(CLAUDE.md 목록과 코드 `[강의 외]` 주석 일치 확인, 빠진 것 없음)
- **(11/22 새벽 B) 큐브 → 직접 만든 모델로 교체 (기능 변화 없음, 모델 교체만)** — 원래 큐브는 지우지 않고 **Mesh Renderer만 끔** (Collider·스크립트·NavMesh 그대로). A가 아는 게 좋은 것:
  ① **`Door` 프리팹**: 자식 `door_steel`(철문: 손잡이·도어스코프·문패·우유 투입구) 추가, 루트 큐브 렌더러 끔 → HingeJoint·DraggableDoor·부수기 그대로 ② **`Breaker` 프리팹**: `Lever` 아래 `breaker_lever`, 루트 아래 `breaker_plate`("주 차단기 ON/OFF"), `Handle` 렌더러만 끔 (Lever Position (0,0,0.06) 확인) ③ **단서 4개**(`Clue_Diary_301`, `Clue_Photo_302`, `Clue_Cassette_302`, `Clue_Note_303`)에 모델 자식, `Clue_Photo_302` ClueItem.image = `photo_302` (읽을 때 사진이 뜸) ④ `Unit` 프리팹에 창문+방범창(`window_unit`), 302호 복도 창 ⑤ 지하: 보일러 4·제사상·촛대·쌀가마니·배관 모델, `Candle_Light` 로컬 Y 0.85, 계단 밑 막는 큐브 2개 ⑥ 계단 손잡이(층마다 A·B), 지하 비상구 표지(Unlit) ⑦ 1톤 트럭 아래·앞 보강 ⑧ 방화 셔터: `FireShutter` 자식 `shutter_curtain`(같이 말려 올라감, 큐브 렌더러만 끔) + 맨 바깥 `shutter_frame`(셔터함·레일) ⑨ 천장 등 모델(복도·보일러실 방수등, 2층 센서등, 관리사무소·302호 형광등 — 각 Light의 자식, Light 컴포넌트는 그대로) ⑩ 복도 벽·바닥 모서리 때 띠 `grime_corridor`(Floor1~3 아래, 문 자리 비움) ⑪ 문 위 `Lintel_*`에 벽지 재질 `wall_wallpaper_lintel`(회색 상자처럼 보이던 것) ⑫ 스프레이 재질 11개 Preserve Specular·반사 끔(투명 부분에 네모가 비치던 것) ⑬ Credits.txt 전부 반영
  → **통플레이 때 볼 것:** 문 열기·괴물 문 부수기, 분전함 레버, 단서 E 읽기가 전과 같은지
- **(11/20) 지하 맵·동생 연출·엔딩 배치 끝 → A 16번(지하 추격) 붙여도 됨.** 302호 `Key_Boiler` 자리도 준비됨(아래). 바깥 발소리는 `Player/FootstepZone.cs`(B가 추가, Trigger로 Player Audio Source clip만 바꿈)
- **Monster_B1**: (17.5, -2.8, 1) 지하 계단 아래(따라 내려온 것처럼), 꺼 둠. 순찰 `B1_Patrol_1~4` = (12, -2.8, 1.5) → (-12, -2.8, 1.5) → (-12, -2.8, 6.8) → (12, -2.8, 6.8) (보일러 줄을 빙 도는 고리)
- **추격 시작 신호** = `BrotherEncounter`가 `activateAfter`의 Monster_B1을 켜고 0.5초 뒤 플레이어 자리에서 `noiseSystem.Emit(…, 40, "동생 울음")`. 다른 신호가 필요하면 `BrotherEncounter.finished`(static)를 보면 됨
- **Checkpoint_4**: (-11.5, -2.8, 4.2) Rotation (0, 90, 0), Box Collider Size (8.4, 3, 2) = 동생 연출 선과 같은 자리 (괴물은 연출 뒤에만 있으므로 여기서 다시 시작해야 추격이 의미 있음)
- **탈출** = `ExitTrigger`(13, -1.6, 10) 들어오면 `EndingScreen`이 Monster_B1을 끄고 엔딩. A가 따로 부를 것 없음. 엔딩 중인지는 `EndingScreen.isEnding`
- **열쇠 문**: `Door_Basement` (1층 계단실 19.05, 1.05, 2.0). 보일러실 열쇠: `Key_Boiler` = 302호 (3.8, 5.6, 7.5), Rotation (0, 30, 0), 모델 `boiler_key`(놋쇠 열쇠 + "보일러실" 꼬리표, Assets/Models/Props/Room302/). 자식 메시에 Box Collider 있음 → **A 14번 열쇠 줍기를 이 오브젝트에 붙이면 됨** (11/20)

- 11/20: 단서 읽기에서 **Q = 이전 장** (A가 B의 `ClueReader.cs`에 추가, 안내 "[Q] 이전"은 2장부터 보임)

## 재질 적용 안내 (11/20 A — SampleScene, 재질을 Hierarchy 오브젝트에 끌어다 놓기)
| 오브젝트 (1·2·3층 각각) | 재질 |
|---|---|
| `FloorN/Slab/Slab_Main`, `Slab_Landing` | `Corridor_Floor` (복도·계단참 바닥. 방 안은 Unit의 장판이 덮음) |
| `FloorN/Corridor/Railing`, `Stairwell/Stair_Railing`, `Railing_StairTop` | `Corridor_Railing` |
| `FloorN/Corridor/Ceiling` | `Interior_Ceiling` |
| 계단실 (1·2·3층 `Stairwell`, `B1/Stairwell (1)`): 계단 `Stair_1~8`, `Landing_Mid`, `Slab_Landing` | `Stair_Concrete` |
| 계단실 벽 `Stair_WallEast`, `Stair_WallNorth`, `Stair_Divider`, 1층 `Basement_Wall_L/R`·`Basement_Lintel`, `B1_WallStair` + 새로 만든 `Stair_WallWest`(1호 벽지 가림판) | `Stair_WallPaint` (초록 페인트 벗겨진 콘크리트) |
| `Unit_304`(벽 3·천장) | 벽 `Interior_Wallpaper_Yellow`, `304_Ceiling` `Interior_Ceiling` (바닥은 Unit 프리팹 `Floor_Jangpan`을 복사해 넣거나 그대로) |
| 1층 관리사무소 바닥·벽 | `Office_FloorTiles`, 벽 `Interior_Wallpaper_Yellow` 또는 그대로 |
| `B1` 벽·바닥 | `Basement_Wall`, `Basement_Floor` |
| (선택) 복도 쪽 벽 `Wall_A~E` | 그대로(콘크리트) — 방 안에서 보이는 앞벽은 콘크리트로 남음 |

## A·B 상의할 것: 스토리 전달 (11/22 A 제안, 아직 안 정함 — 다음에)
**1) 시작할 때 스토리를 어떻게 알려 줄지** (처음엔 "누구·왜·뭘 할지"만, 30년 전 사건·반전은 단서로)
- ① **인트로 자막**(타이틀 "시작하기" 직후 약 15초, 아무 키로 건너뛰기, OnGUI + 코루틴): "2024년 12월. 철거를 일주일 앞둔 ○○아파트. / 나는 빈집 확인 용역이다. 오늘 밤 마지막 점검만 끝내면 된다. / 주민은 모두 떠났다. 전기도 끊겼다. / …그런데 302호에 불이 켜져 있다." → 바깥으로 밝아지며 시작 (정면에 302호 불빛)
- ② **경비실 앞 "공가 확인 작업 지시서"**(ClueItem, 코드 수정 없음): 관리사무소에서 손전등·열쇠 챙기기 / 전 세대 공가 확인 / ~~302호~~ (지운 흔적, "그 집은 건드리지 말 것")
- ③ (선택) 화면 구석 **현재 목표 한 줄**: 관리사무소 → 302호 확인 → 지하 보일러실 (발표 때 헤매지 않게)
- 연도는 **현재 = 2024년 겨울**(일기 1994년 + 30년, 축문 "2024년까지"와 맞음)

**2) 주인공이 왜 자기가 살던 아파트를 기억 못 하나** (반전 설득력)
- 사실 정리: 1994년 12월 주인공(302호 장남) 7살 → 축문에서 장남 이름에 줄, 차남(동생)으로 바뀜 → 지금 37살
- 안 A (현실): 7살 기억이라 흐릿 + 동생이 사라진 다음 날 가족이 급히 이사, 엄마는 동생 얘기를 안 함("형아는 멀리 갔대" 식으로 덮음) → 용역 회사가 배정한 현장이 하필 그곳인 줄 모르고 옴. 복도식 아파트는 다 비슷함
- 안 B (A + 초자연, 선택): "집주인에게 바쳐진 아이는 사람들의 기억에서 지워진다" → 주민도 주인공도 동생이 있었다는 것 자체를 잊음. 302호 불 = 동생이 "형이 나를 잊지 않게" 켜 둔 것. 지하에서 동생과 마주하며 기억이 돌아옴
- 보여 주는 법: 인트로에 "…이 동네, 어릴 때 와 본 것 같기도 하다" 한 줄만 / **주인공 혼잣말 자막**(Trigger + 안내 문구, TutorialHint 방식): 302호 문 앞 "…이 문, 손잡이 높이가 기억난다", 신발 "작은 고무신… 누구 거였지", 사진 "이 큰 아이… 나다" / 지하 동생 앞 "…그날 밤, 내가 갔어야 했다"
- 정해지면 CLAUDE.md 4장 스토리에 반영. 스토리·타이틀은 B 담당이라 같이 정하고, 인트로 스크립트는 A가 만들어도 됨

## 안 쓰는 파일 정리 (11/22 A가 조사만 함, 아직 안 지움 — 동결 후 정리 예정)
저장된 SampleScene 기준으로 씬·프리팹·재질 참조를 따라가 본 결과. **지울 때는 반드시 Unity Project 창에서**(.meta 같이), 지운 뒤 통플레이로 Missing 경고 확인, Credits 줄도 정리.
- ✅ **지워도 됨 (A가 넣은 것)**: Audio 25개(`breath_calm_b`, `breath_heavy_a`, `footstep_concrete_001~004`, `footstep_snow_001~004`, `creak2·3`, `doorClose_2·3`, `doorOpen_2`, `door_break_b`, `door_knock_b`, `impactMetal_light_001~004`, `impactWood_medium_001~004`) / 안 놓은 Poly Haven: `korean_public_payphone_01`, `metal_trash_can`, `power_box_01`(모델·텍스처·재질) + 프리팹 `Prop_Payphone`·`Prop_TrashCan`·`Prop_PowerBox` / `portable_cassette_player_alpha_1k.png`, `wall_clock_glass_alpha_1k.png` / 재질 `Interior_Linoleum`
- ⚠️ **B에게 확인 (B가 넣은 것)**: `Textures/Walls/corridor_wall_basecolor·normal.png`(17.5MB, 재질에 연결 안 됨), `cracked_concrete_wall_*`, `Graffiti/graf_bosanghara·churipgeumji·jipjuin_x·nagara`, `Sprays/gongga_spray.png`, `car_paint_white.jpg`, `Audio/Story/brother_musicbox.ogg`, 재질 `maker material`·`wall_concrete(_long/_short)`·`wall_wallpaper_long/short`·`clue_cassette_material`·`clue_photo_material`·`lamp_tube`·`car_white`·`wardrobe_inside`·`gongga_spray`·`graf_bosanghara`·`graf_nagara`, 프리팹 `street_lamp.prefab`(`StreetLamp`와 중복?)
- ❌ **지우면 안 됨 (검사엔 안 씀으로 나오지만 필요)**: 모든 `.mtl`(OBJ 짝 파일), `Models/Brother/Texture/*`(동생 모델이 이름으로 찾아 씀), `Fonts/OFL.txt`(글꼴 라이선스), `Scenes/Test_A`(A 테스트 씬), `TutorialInfo`·`Readme.asset`·`Settings`(템플릿 제공)

## 알려진 문제
**(11/24 A·B 통플레이 메모) — 아직 안 고침**
| # | 내용 | 누가 | 메모 |
|---|---|---|---|
| 1 | 1층 관리사무소 안에서 앞으로 안 움직이는 곳 없애기 | A·B | 🟡 **B가 씬 파일로 원인 후보 찾음(11/24)**: `Prop_SteelShelves`(-15.7, 0, 5.2)·`Prop_Chair`(-11.17, 0, 6.73)가 Rotation X -90으로 눕혀져 있어 **Box Collider도 같이 누움** → 바닥에 높이 25~28cm 판(선반: x -15.7~-13.6, z 4.6~5.8)이 깔려 못 넘어감. 모델은 서 있는데 콜라이더만 누웠으면 콜라이더 Size Y↔Z 바꾸고 Center (0, 0, Z/2) (선반: Size (1.097, 0.502, 2.141), Center (0, 0, 1.07)) / 모델도 쓰러져 있으면 Rotation (0, 90, 0)으로 세우기 → NavMesh 다시 Bake |
| 2 | 2층에 올라오자마자 "레버를 올려야 한다" 안내 띄우기 + 레버를 올리면 화면 고정 | B(안내)·A(화면 고정) | 🟡 **화면 고정 A 스크립트 완료(11/24), 테스트 전**: `Floor2Event` 새 칸 `Lock View During Event`(켜짐) — 레버를 올린 순간 레버를 놓고 걷기·마우스 멈춤 → 1.2초 동안 고개가 괴물(Monster_2F) 가슴 쪽으로 돌아감 → 괴물이 걸어와도 계속 따라 봄 → "숨어라"와 동시에 다시 움직임. 체크포인트 2에서 다시 시작할 때도 같음. `PlayerLook.SetLook/Pitch`, `PlayerInteract.ReleaseGrab` 추가. 안내 문구는 B |
| 2-1 | 불이 다 켜진 뒤 "숨어라"까지 너무 김 (약 4.7초) | A (씬 값) | ⏸ **B가 씬 작업을 끝내고 push한 뒤** A가 pull → `Floor2Event` **Flicker Start Delay 1.5 → 0.7**, **Flicker Time 2.5 → 1.5** (약 2.9초) → 커밋·push. 씬 동시 수정 충돌 방지 |
| 3 | 장롱 안에 숨었을 때 괴물 발소리가 들리게 | A | Monster 발소리 Audio Source 거리·볼륨 확인 |
| 4 | 장롱에 숨었을 때 괴물이 갔는지 화면에 띄우기 | A | 예: "발소리가 멀어졌다" |
| 5 | 괴물이 너무 느림 | A | 🟡 **A 완료(11/24), 테스트 전**: `Monster` 프리팹 Patrol 1.2 → **1.6**, Investigate 2.2 → **3**, Chase 3.5 → **4.3** (플레이어 걷기 3 < 추격 4.3 < 달리기 5.5 → 걸으면 잡히고 뛰면 도망). 씬의 Monster·Monster_2F·Monster_B1에 속도 덮어쓰기 없음 → 셋 다 바뀜 |
| 6 | 괴물이 소리를 듣는 범위가 너무 작음 | A | 🟡 **A 완료(11/24), 테스트 전**: `MonsterAI` 새 칸 **Hearing Multiplier = 1.5** (소리 크기 × 1.5 거리까지 들음: 달리기 6 → 9m, 물건 떨어짐 8 → 12m, 셔터 20 → 30m). 다른 층 소리는 그대로 무시 |
| 7 | 마지막(지하) 도망쳐 나올 때 쫓기는 배경음 추가 | A | 🟡 **A 완료(11/24), 테스트 전**: `BrotherEncounter` 새 칸 **Chase Music Until End**(켜짐) — "도망쳐"와 함께 추격 음악이 켜져 엔딩까지 계속(괴물이 놓쳐도 안 꺼짐), 체크포인트 4 재시작도 같음. 엔딩 화면이 줄여 끔 (B 파일이지만 칸 하나만 추가) |
| 8 | 타이틀 화면이 반투명이라 허술해 보임 | B | 🟡 B(11/24) 코드 완료, Unity 확인 전: 불투명 + 배경 그림 `title_background`(게임 시작 지점 캡처를 보정, 302호 불빛, 은은하게 천천히 확대) + 버튼 3개 "시작하기 / 조작법 / 그만하기", 조작법 표(키·설명은 Inspector 배열). Background Brightness 0.85. 글꼴은 후보만 정함(제목 동해독도 + 본문 송명 추천) |
| 9 | 2층 도착 "레버를 올려야 한다", 3층 체크포인트 "302호 열쇠 → 보일러실" 안내 | B | 🟡 B(11/24) 배치: TutorialHint 복사 — `Hint_Lever`(17.6, 4.0, 0.9) Size (3.6, 2.4, 2), Skip 2 / `Hint_Key`(15, 6.8, 1) Size (2, 2.4, 2), Skip 4 |

- (11/22 A) **분전함이 두 겹으로 보임** → A의 `Model_PowerBox`와 B의 `breaker_plate`가 겹침 → `Breaker` 프리팹에서 A 모델 뺌
- (11/22 A) **분전함 레버가 안 잡힘** → ① 씬에서 `Handle` 오브젝트가 통째로 꺼져 있어 Box Collider도 꺼짐 → `Handle`은 켜고 **Mesh Renderer만 끔**(사용자가 씬에서 고침) ② `Breaker` 프리팹 Handle Box Collider를 Size (2.5, 2.5, 1.3), Center z -0.05로 키움(B 레버 모델을 감쌈)
- (11/22 A) **2층 "숨어라" 중에 1층으로 내려가면 연출이 꼬임** → `Floor2Event`에 **Stair Blockers** 추가: 레버를 올리는 순간(다시 시작 포함) 1층 내려가는 계단 입구의 투명 벽 `StairBlock_2F`를 켜고, 괴물이 지나가 셔터가 열릴 때 끔. 부딪히면 `Story/BlockedMessage.cs`가 "지금은 내려갈 수 없다. 어딘가에 숨어야 한다". 벽은 Mesh Renderer 없는 Box Collider라 괴물(NavMesh·Kinematic)은 통과·Bake에도 안 들어감. 씬 배치: `Floor2/Stairwell/StairBlock_2F` 로컬 (19.05, 1.3, 2.05), Box Collider Size (1.95, 2.6, 0.1), 처음엔 꺼 두고 Floor2Event Stair Blockers에 연결
- (11/22 A) **3층 복도 상자(Box)가 안 보이고 잡히기만 함** → A가 넣은 골판지 모델의 FBX 기본 크기(100배)·회전(-90°)을 덮어써서 아주 작아짐 → Box 프리팹 모델 Scale 250, Rotation X 270, 위치 (0, 0.03, -0.08), 콜라이더 0.39×0.34×0.52로 바로잡음. 다른 `Prop_*`도 눕거나 작으면 같은 원인
- (11/22 A) `RoarScare`의 `FindFirstObjectByType` → `FindAnyObjectByType` (Unity 6.5 사용 중단 경고)
- (11/22) 302호 카세트 동생 목소리(음성 합성)는 만들어 봤다가 **사용자 결정으로 안 쓰기로 함** — 카세트는 글만
- ✅ **(11/21, B 발견·수정) Poly Haven 소품이 바닥에 절반 묻힘** → `Prop_TrashBag`, `Prop_CardboardBox`는 모델이 Box Collider 아래로 내려가 있었음. 프리팹 자식을 고침: `cardboard_box_01` Position Y 0.171, `trashbag`은 Rotation X 270으로 돌려서 바닥에 맞춤 (204·303·304·1층 복도가 같이 고쳐짐). 다른 `Prop_*`도 같은 증상이면 자식 Y = Box Collider Size Y ÷ 2. 집는 `Box` 프리팹도 같은 모델이라 확인 필요
- **(11/20) 3층 계단실 바닥이 뚫려 보임/떨어짐** → B가 지붕 계단을 지울 때 3층 `Ramp_A·Ramp_B·Landing_Mid·Stair_Divider`만 지우고 **보이는 계단 `Floor3/Stairwell/Steps_A·Steps_B`(Collider 없음)는 남음** + 2층→3층 계단 위 서쪽 절반(x 16.1~18, z 2~)이 빈 구멍. A(사용자)가 SampleScene에서: 3층 Steps_A·B 끄기 + `Floor3/Stairwell`에 낮은 벽 2개(`Railing_StairTop` 로컬 (17.05, 0.55, 1.9) 크기 (1.9, 1.1, 0.2) / `Railing_StairDivider` 로컬 (18.05, 0.55, 3.15) 크기 (0.2, 1.1, 3)) → NavMesh 다시 Bake
- ✅ **(11/20 A 해결) 2층에서 잡혔는데 지하 제사상에서 다시 시작됨** → `Checkpoint_4`(지하 y -2.8)의 Trigger가 높이 3m라 위쪽이 1층 바닥 위 0.2m까지 올라와, **1층 관리사무소를 걸을 때 체크포인트 4가 저장**되고 있었음. Checkpoint 프리팹 Box Collider를 Size Y 3 → **2.4**, Center Y 1.5 → **1.2**로 낮춤(위층 바닥과 겹치지 않게, 모든 체크포인트 공통). 체크포인트를 새로 놓을 때 Trigger 위쪽이 위층 바닥(+2.8)을 넘지 않게
- ✅ B 해결(11/21): **(11/20, A 발견) 바깥 나무 가지가 2·3층 복도 NavMesh를 끊음** → 2층 괴물이 길이 끊겨 202호 문을 부수고 들어감. NavMesh Surface가 Render Meshes로 굽기 때문에 복도 높이까지 뻗은 `dead_tree_1~3` 가지가 장애물로 들어감. 해결: `dead_tree_1~3` 프리팹 루트에 **NavMesh Modifier(Mode = Remove object / Ignore From Build)** → 다시 Bake → 1~3층 복도 파란 바닥이 끊김 없이 이어졌는지 확인. (B: 나무가 건물 벽·복도 안으로 파고들지 않게 조금 남쪽으로 옮기면 더 좋음) → B: Tree_1~3 모두 NavMesh Modifier 확인, 화단은 건물에 붙인 채로 나무를 돌려 가지가 남쪽을 보게 함(Tree_1 Y 270, Tree_2 Y 290, Tree_3 Y 70·Scale 0.8) → 다시 Bake
- ~~**(11/18, B에게) 계단실 맨 위 계단 없애기**~~ ✅ B 해결(11/20): 3층에서 지붕(8.4) 쪽으로 올라가는 마지막 계단(Floor3/Stairwell의 Steps_B·Ramp_B 등)이 남아 있음 → 3층이 꼭대기라 위로 가는 계단은 지우거나 꺼 주기. 지운 뒤 NavMesh 다시 Bake
- (11/17) 문 위 벽(Lintel) 아래 NavMesh가 끊겨 괴물이 세대 안으로 못 들어가고 문 앞에서 멈춤 → **Navigation Agents > Humanoid Height 2 → 1.8**(괴물 키 1.9로 줄임과 함께) + 다시 Bake. 맵을 고치면 Bake 후 Scene 창 Show NavMesh로 문 앞이 이어졌는지 확인
- ✅ B 해결(11/20): (11/17, B에게) 문 위 벽 `Lintel_101~304`, `Lintel_Office`의 Scale Z가 2라서 복도 쪽으로 0.9m 튀어나와 천장에 상자가 매달린 것처럼 보임 → **Scale Z를 0.2로** (Position z 2.1 그대로). 괴물이 여기에 파묻혀 보여서 A가 Monster 프리팹을 키 1.9m·폭 0.6m로 줄임(2층·3층 공통)
- (가끔) 상자를 들고 고개를 빠르게 돌린 뒤 놓으면 멀리 튕겨 나감 → 상자가 플레이어 몸을 통과하며 겹쳐서 생기는 현상. 다시 생기면 `Physics.IgnoreCollision` + `maxDepenetrationVelocity`(강의 외)로 고치기로 함

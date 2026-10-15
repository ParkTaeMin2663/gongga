# CLAUDE.md — 「공가(空家)」 텀 프로젝트

이 파일은 Claude Code가 이 프로젝트에서 작업할 때 항상 따라야 하는 규칙과 게임 설계 문서다.
답변과 코드 주석은 한국어로 작성한다.

## 1. 과제 규칙 (위반 시 감점 또는 F — 최우선으로 지킬 것)

### 엔진과 프로젝트
- **Unity 버전: 6000.5.1f1 고정.** 다른 버전으로 제출하면 10% 감점, 변환이 제대로 안 되면 10% 추가 감점.
  - `ProjectSettings/ProjectVersion.txt`로 버전을 확인한다. 이 버전에 없는 API나 패키지는 쓰지 않는다.
- **이클래스에 올라온 'Template Project'를 기반으로 구현한다.** 새 프로젝트를 만들어 제출하면 10% 감점.
- **SampleScene을 메인 씬으로 사용한다.** 이름 변경 금지, 삭제 금지. Build Settings에서 첫 번째 씬(index 0)이어야 한다.
  - 추가 씬이 필요하면 만들 수 있지만, 게임은 반드시 SampleScene에서 시작해야 한다.
- **SampleScene의 'Main Camera'는 절대 삭제하지 않는다.**
  - 1인칭 플레이어 카메라는 새 카메라를 만들지 말고, **기존 Main Camera를 플레이어 오브젝트의 자식으로 옮겨서** 사용한다.
  - Main Camera의 이름과 태그(MainCamera)도 유지한다.
- **SampleScene의 'Global Volume'은 절대 삭제하지 않는다.** (광원·후처리 설정이 연결되어 있음)
  - `Assets/Settings/GlobalVolumeSettings.cs`도 템플릿 제공 파일이므로 옮기거나 삭제하지 않는다.

### 스크립트 위치
- **우리가 직접 작성한 스크립트는 전부 `Assets/MyScripts/` 안에 둔다.** (하위 폴더는 허용)
- 인터넷 등에서 가져와서 수정한 스크립트는 `Assets/OtherScripts/`에 두고, 파일 상단 주석에 출처를 적는다.
- 이 두 폴더 밖에 .cs 파일을 만들지 않는다.

### 리소스
- **유료 애셋, 유료 리소스 사용 금지.** 직접 만들거나 무료 리소스만 쓴다.
- 무료 리소스(사운드, 텍스처, 폰트 등)를 쓰면 `Assets/Credits.txt`에 이름, 출처 URL, 라이선스를 기록한다.
- **기존 프로젝트(참고서, 인터넷 공개 프로젝트)를 가져와서 수정하는 방식은 금지.** 처음부터 직접 만든다.

### 일정과 평가
- 10/27: 텀 프로젝트 계획 발표
- **12/01 08:30: 최종 제출** (프로젝트 소스 파일 + 발표 자료, 팀 대표 1명 제출).
- 12/01, 12/08: 발표
- 평가: **창의성 50%, 완성도/기술성 50%** (UI의 미려함, 버그 유무 포함)
- 2인 1조 → 평가 점수 5% 가산점
- AI 사용은 허용됨.

---

## 2. 작업 방식

- 팀원 2명이 역할을 나누지 않고 함께 작업한다. 주차별 목표(5장 일정)를 기준으로 진행한다.
- Git + GitHub로 협업한다. `.meta` 파일은 반드시 함께 커밋한다.
- 씬 파일(.unity)은 두 사람이 동시에 수정하면 충돌이 심하므로, 기능은 가능한 한 **프리팹 단위**로 만들고 씬 수정 전에는 서로 알린다.
- Claude Code는 Unity 에디터를 직접 조작할 수 없으므로, 에디터에서 해야 하는 작업(오브젝트 배치, 컴포넌트 연결, 레이어/태그 설정, NavMesh 굽기 등)은 **사용자가 따라 할 수 있게 단계별로 설명**한다.
- 코드를 바꾼 뒤에는 Unity에서 무엇을 확인하면 되는지 짧게 알려준다.

---

## 3. 코딩 규칙

- C#, Unity 6 기준. 시작 전에 **Project Settings > Player > Active Input Handling** 설정을 확인하고(구 Input Manager / 새 Input System / Both), 그에 맞는 입력 코드를 쓴다.
- Unity 6 API를 쓴다. (예: `Rigidbody.velocity` 대신 `Rigidbody.linearVelocity`, `FindObjectOfType` 대신 `FindFirstObjectByType`)
- 클래스 하나당 파일 하나, 파일명 = 클래스명.
- 인스펙터에서 조절할 값은 **강의 방식대로 `public` 변수**로 노출하고 `[Header]`, `[Tooltip]`으로 한국어 설명을 단다. 인스펙터에 보일 필요 없는 값은 `private`. (6장 참고)
- 강의 권장대로 `this.`를 붙여 쓴다. 변수는 소문자로 시작, 함수·클래스는 대문자로 시작.
- 매 프레임 `Find`, `GetComponent` 호출을 피하고 캐싱한다.
- 다른 시스템과 연결되는 부분은 이벤트나 인터페이스로 느슨하게 연결한다. (예: `IInteractable`, 소리 발생은 `NoiseSystem.Emit(position, loudness)`)
- 폴더 구조:
  ```
  Assets/
    MyScripts/
      Core/         게임 상태, 챕터 진행, 세이브/체크포인트
      Player/       이동, 카메라, 손전등
      Interaction/  IInteractable, 문/서랍/레버 드래그, 집기/던지기, 숨기
      Monster/      집주인 AI, 소리 감지
      Noise/        소리 발생 시스템
      Story/        단서, 연출(2층 이벤트 등), 엔딩
      UI/           HUD, 단서 읽기 화면, 메뉴
    OtherScripts/
    Prefabs/  Materials/  Audio/  Textures/  Scenes/
    Credits.txt
  ```

---

## 4. 게임 설계

### 개요
- 제목: **「공가(空家)」**
- 장르: 1인칭 공포 어드벤처 (암네시아: 더 다크 디센트 스타일의 물리 상호작용 + 무기 없는 생존)
- 배경: 철거를 일주일 앞둔 1980년대식 한국 **복도식 아파트** 단지, 눈 내리는 겨울밤
- 분위기: 차가운 회색/남색 바탕에 빨간 "공가" 스프레이와 노란 불빛만 튀게. 안개와 어두운 조명.

### 스토리
- 주인공은 철거 전 빈집 확인 업무를 맡은 용역 직원. 전부 비었어야 할 단지인데 **302호에만 불이 켜져 있다.**
- 남겨진 일기장, 사진, 카세트테이프를 조사할수록 30년 전 이 단지에서 한 아이가 사라진 사건이 드러난다. 주민들은 해마다 "집주인"에게 아이를 바쳐왔고, 그 일을 덮은 채 떠났다.
- **반전:** 그해 바쳐질 아이는 원래 주인공이었고, 대신 끌려간 건 동생이었다. 302호의 불은 30년 동안 형/누나를 기다린 동생이 켜두던 것. 지하 보일러실에서 동생과 마주하며 결말을 맞는다.

### 괴물: 「집주인」
- 옛날 집 지킴이 신이 뒤틀린 존재. 주민들이 30년간 제사상으로 달래왔지만, 모두 떠나자 굶주린 채 남았다.
- 외형: 뜯겨 나간 **벽지, 장판, 녹슨 문패**가 사람 모양으로 뭉친 형체. 천장에 닿을 만큼 커서 구부정하게 기어 다닌다. (유료 애셋 금지 → 찢어진 Plane 여러 장을 붙이고 흔드는 방식으로 직접 제작)
- **눈이 없어서 소리로만 사냥한다.**
- AI 상태: 순찰 → (소리 감지) 의심/이동 → (플레이어 근접) 추격 → (놓침) 수색 → 순찰
- 닫힌 문 앞에서는 몇 초간 두드리다 부수고 들어온다.
- 장롱에 숨은 플레이어는 찾지 못한다. (단, 숨기 직전에 소리를 냈다면 확인하러 올 수 있음 — 여유 있으면 구현)
- 괴물은 **3층과 지하에서만** AI로 동작한다. 2층에서는 정해진 연출로만 움직인다.

### 맵 구성
1층 관리사무소와 복도 → 2층 복도와 201호 → **방화 셔터로 막힌 계단** → 3층 복도와 301~304호 → 지하 보일러실
- 복도식 아파트는 층 하나를 만들고 복사·변형해서 제작한다.
- **층 구조 (모든 층 공통):** 복도 x -16~16, z 0~2 / 세대 z 2.2~8.2 / 계단실은 동쪽 x 16~20
  - 계단에서 올라와 서쪽을 보면 **처음 보이는 집이 1호, 복도 끝이 4호**
  - 문 위치(x): 1호 = 10, 2호 = 2, 3호 = -6, 4호 = -14
- **1층:** 101·102·103 잠김, 4호 자리 = **관리사무소** (손전등·열쇠 꾸러미). 플레이어 시작 (14, 0, 1), 회전 (0, -90, 0)
- **2층:** 201 = 장롱 (숨기), 202·203 잠김, 204 = 문이 열려 있고 그 앞 복도 끝에 집주인 실루엣. 계단 옆 분전함, 3층 계단에 방화 셔터, 센서등
- **3층:** 301 = 장롱·일기장, 302 = 불 켜진 집 (사진·카세트테이프·보일러실 열쇠), 303 = 서랍 단서, 304 = 괴물 순찰 시작점 + 추격 중 숨는 두 번째 장롱
- **지하:** 보일러실, 서쪽 끝 제사상과 동생, 비상구 탈출
- 각 세대 문에는 빨간 스프레이 "공가" 표시.

### 챕터
| 챕터 | 장소 | 내용 | 괴물 |
|---|---|---|---|
| 1장 | 1층 | 관리사무소에서 **손전등과 열쇠 꾸러미 획득**, 302호 불빛 발견, 조작 튜토리얼 | 위층에서 소리만 |
| 2장 | 2층 | 3층 계단이 방화 셔터로 막혀 있음 → 분전함 차단기를 올림 → 셔터가 "끼이익" 올라가는 소리 + 센서등이 하나씩 켜지며 복도 끝에 **집주인 실루엣** → 손전등이 깜빡이며 꺼지고 기어 오는 소리 → "숨어라" 힌트 → **201호 장롱에 숨기** → 괴물이 지나간 뒤 3층으로 | 연출만 (AI 없음) |
| 3장 | 3층 | 네 세대 탐색, 302호에서 동생의 흔적, 첫 추격 | AI 추격 |
| 4장 | 지하 | 30년간의 제사상과 진실, 동생과 마주함, 최종 추격 → 탈출 엔딩 | 최종 추격 |

공포 단계: 소리만 → 실루엣 → 추격 → 최종 추격

**목표 플레이타임: 첫 플레이 기준 약 20~30분** (1장 3~5분, 2장 3~5분, 3장 8~12분, 4장 5~8분). 발표 시연은 2장 연출과 3장 추격 위주로 5분 내외.

### 시스템
- **손전등 배터리:** 켜면 줄어들고, 결정적인 순간(2층 등) 깜빡이며 꺼지는 연출이 있다.
- **무기 없음:** 숨기(장롱)와 시간 벌기(문 닫기)로만 생존한다.
- **물리 상호작용:** 문, 서랍, 차단기 레버는 **마우스로 잡고 끌어서** 조작한다 (암네시아 방식, Rigidbody/Joint 기반). 물건은 집고, 회전시켜 보고, 던질 수 있다.
- **소리 규칙:** 뛰기, 물건 떨어뜨리기/던지기, 셔터, 센서등 같은 큰 소리가 괴물을 부른다. 던진 물건으로 괴물을 다른 곳에 유인할 수 있다.
- **단서:** 일기장, 사진, 카세트테이프를 읽거나 들을 수 있다.
- **사망:** 잡히면 짧은 연출 후 마지막 체크포인트에서 재시작.
- **엔딩:** 1개 (여유 있으면 2개)

---

## 5. 개발 단계 (2인 팀 기준, 우선순위 순)

1. **필수 (MVP):** 맵 4개 구역, 1인칭 이동, 손전등, 문/서랍 드래그, 단서, 2층 연출, 단순 추격하는 괴물, 엔딩 1개 → 이것만으로 제출 가능해야 한다
2. **목표:** 소리 감지, 장롱 숨기, 문 부수기, 물건 던져 유인, 체크포인트
3. **여유 있으면:** 엔딩 2개, 괴물 외형 디테일, 연출 다듬기, 사운드 강화

항상 **지금 단계가 먼저 동작하는 상태**를 유지하고 다음 기능을 얹는다.

### 일정
| 기간 | 할 일 |
|---|---|
| 10/6 ~ 10/12 | (중간고사 주) 기획 확정, Template Project와 Git 세팅, 폴더 구조 만들기 |
| 10/13 ~ 10/19 | 프로토타입: 1인칭 이동, 손전등, 문 드래그 열기, 회색 박스로 1층 맵 뼈대 |
| 10/20 ~ 10/26 | 10/27 계획 발표 준비 (컨셉, 맵, 일정, 프로토타입 시연 영상) |
| 10/27 ~ 11/2 | 상호작용 완성(서랍, 레버, 물건 집기/던지기), 1층·2층·3층 맵 |
| 11/3 ~ 11/9 | 단서 UI(일기, 사진, 테이프), 손전등 배터리, 2층 연출(셔터, 센서등, 실루엣), 괴물 기본 추격 |
| 11/10 ~ 11/16 | 소리 감지, 장롱 숨기, 문 부수기, 사망과 체크포인트, 지하 맵 |
| 11/17 ~ 11/23 | 스토리 연출, 엔딩 연결, 타이틀/엔딩 화면, 괴물 외형, NavMesh 다듬기 (11/17 수업 반영) |
| 11/24 ~ 11/30 | 버그 수정, 밸런스, 처음부터 끝까지 통플레이 테스트, 발표 자료, 최종 확인 |
| **12/01 08:30** | **제출** |

---

## 6. 강의자료 기준 구현 (최우선 참고)

사용자는 **강의(02·03·04장)에서 배운 방식으로 최대한 구현하기를 원한다.** 코드를 설계할 때 항상 이 장을 먼저 본다.

- **원칙:** 아래 "강의에서 배운 것"에 있는 기능·API·에디터 방식을 먼저 쓴다.
- **강의에 없는 기능**이 꼭 필요하면, 코드를 쓰기 전에 ① 왜 필요한지 ② 강의 방식으로는 왜 안 되는지 설명하고 **사용자 확인을 받은 뒤** 쓴다. 코드에는 `// [강의 외]` 주석으로 표시한다.
- 설명할 때 "강의 03장의 OOO처럼"과 같이 강의 내용과 연결해서 설명한다.

### 강의에서 배운 것

**02 유니티 개발환경 준비**
- 입력: `Player Input` 컴포넌트 + 메시지 함수 `OnMove`, `OnLook`, `OnAttack`, `OnInteract`, `OnCrouch`, `OnJump`, `OnSprint`(`InputValue value`)
- Interactions: `Press`(Trigger Behavior: Press Only / Release Only / **Press And Release**), `Hold`(Hold Time), `Tap`, `Slow Tap`, `Multi Tap`(Tap Count) → 버튼을 뗀 순간이 필요하면 **Press And Release + `value.isPressed`**
- `Start()`(1회) / `Update()`(매 프레임), `Debug.Log()`, `this`
- `public` 변수 = Inspector에서 조회·수정, `private`/`protected` = 숨김
- 조명 3종: Point Light, Directional Light, **Spotlight**(spot angle)
- Console 옵션: Clear on Play, Error Pause 켬 / Play 중 바꾼 값은 종료 시 원래대로 / `Ctrl+S` 습관화

**03 게임 오브젝트 다루기**
- Transform: `position`, `Translate(이동량, Space.Self/World)`, `rotation` + `Quaternion.Euler`, `Rotate`, `localScale`
- `Time.deltaTime`으로 프레임과 무관한 속도, `Vector3.forward` 등 상수, `.normalized`, `Random.Range`
- `Rigidbody`: Mass, Linear Damping, Angular Damping, Use Gravity, Is Kinematic, Interpolate, Collision Detection(Discrete/Continuous), **Constraints**(Freeze Position/Rotation)
- `AddForce`로 힘을 주어 발사
- Physics Material: Bounciness, Bounce Combine
- Material(Base Map), Texture, 원본 색 유지 시 `Universal Render Pipeline/Unlit`
- `AudioSource`(Play On Awake, Loop, Priority, Pitch), `this.GetComponent<AudioSource>().Play()`
- 충돌: `OnCollisionEnter/Stay/Exit(Collision coll)`, `coll.gameObject.name`
  - 발생 조건: 두 물체 모두 Collider + 최소 하나는 Rigidbody(Is Kinematic 꺼짐)

**04 Prefab 다루기**
- 부모-자식 계층구조(자식은 부모 기준 상대 좌표), `transform.parent`, Empty Object로 그룹화
- `GameObject.Find()`는 **`Start()`에서 한 번만** (Update에서 쓰지 않기)
- Prefab 만들기(Project 탭으로 드래그), 사본 값 변경 시 굵은 글씨, `Revert`, `Instantiate`
- `OnGUI()`로 화면에 글자·이미지 표시 (GUI 함수는 OnGUI 안에서만)
- 새 Scene, `File > Build Profiles`에 Scene 등록, `SceneManager.LoadScene(이름/인덱스, Single/Additive)`, 현재 Scene 다시 로드로 재시작
- 새 Input Action 만들기 (예: `Fire` → `OnFire`, Press + Press And Release)
- `RigidbodyConstraints.FreezeAll` / `FreezePosition` / `FreezeRotation` (코드로 제약)

### 「공가」 기능 ↔ 강의 방식 대응

| 기능 | 강의 방식으로 구현 |
|---|---|
| 1인칭 이동 | `Rigidbody` + Constraints(Freeze Rotation) + `OnMove` |
| 달리기 | Sprint 액션에 Press(Press And Release) → `OnSprint`에서 `value.isPressed` |
| 마우스 시점 | `OnLook` + `transform.Rotate` (Player는 좌우, 자식 Main Camera는 상하 — 계층구조) |
| 손전등 | Main Camera의 자식 **Spotlight**, 새 Input Action으로 켜고 끄기 |
| 물건 던지기 | `Rigidbody.AddForce` |
| 물건 떨어지는 소리 | `OnCollisionEnter` + `AudioSource.Play()` |
| 맵 반복 구조(복도·세대) | Prefab + Empty Object 그룹화 |
| 재질·바닥 | Material, Texture, Physics Material |
| 안내 문구·단서 글 | `OnGUI()` |
| 사망 후 재시작 | `SceneManager.LoadScene`으로 현재 Scene 다시 로드 |

### 강의에 아직 없는 것 (쓰기 전에 사용자 확인)
- **사용자가 새 강의자료를 올리면**, 거기서 다룬 기능이 이미 `// [강의 외]`로 쓴 코드에 있는지 찾아서 강의 방식으로 고친다. (현재 사용 중: `Physics.Raycast`, `HingeJoint`, `ForceMode.Impulse`, `interface` — PlayerInteract, DraggableDoor, IInteractable / 커서 잠금 — PlayerLook)
- `HingeJoint` 등 Joint (문·서랍·레버 드래그)
- `Physics.Raycast` (바라보는 물체 찾기)
- `OnTriggerEnter` / Is Trigger (구역 진입 감지, 장롱 숨기)
- `NavMeshAgent` (괴물 추적 — 일정상 11/17 수업에서 다룰 예정)
- `CharacterController`, UI(Canvas/TextMeshPro), Timeline
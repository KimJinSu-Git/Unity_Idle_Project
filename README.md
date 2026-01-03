# 📌 Unity 2D 모바일 방치형 RPG – Idle Slayer
🛠 개발 도구: Unity 6000.0.51f1, C#, JetBrains Rider <br/>
📆 개발 기간: 25.12.01 ~ 25.12.23 (약 3주) <br/>
___
Idle Slayer는 자동 전투와 오프라인 보상을 기반으로 한 모바일 방치형 RPG입니다. <br/>
플레이어는 끊임없이 등장하는 몬스터를 처치하며 재화를 획득하고,  <br/>
장비 뽑기와 강화, 퀘스트를 통해 지속적인 성장을 경험할 수 있습니다. <br/>
___
📸 **인게임 이미지**
<p align="center">
  <img src="https://github.com/KimJinSu-Git/Unity_Idle_Project/blob/main/Assets/Screenshots/Capture1.png" width="220"/> &nbsp; &nbsp; &nbsp; &nbsp;
  <img src="https://github.com/KimJinSu-Git/Unity_Idle_Project/blob/main/Assets/Screenshots/Capture2.png" width="220"/> &nbsp; &nbsp; &nbsp; &nbsp;
  <img src="https://github.com/KimJinSu-Git/Unity_Idle_Project/blob/main/Assets/Screenshots/Capture3.png" width="220"/>
</p>

___
# 🔧 주요 구현 시스템
🧩 **전체 시스템 구조**
* **GameManager**
  * 게임 전체 흐름 오케스트레이션
  * 앱 생명주기 기반 자동 저장 👉 []()
  * 데이터 리셋 시 예외 처리
* **DataManager**
  * 비동기 저장 / 로드
  * 로드 실패 시 안전한 신규 데이터 생성
* **StageManager**
  * 스테이지 진행 관리
  * Addressables 기반 몬스터 / 배경 로드
⚔️ **전투 & 배속 시스템**
* **BattleManager**
  * 게임 속도(배속) 중앙 관리
  * 속도 변경 시 이벤트 발행
  * 이벤트 기반 배속 동기화
    * Player / Monster / Background가 이벤트를 구독하여 각자의 로컬 속도를 갱신
  * 자동 전투 구조
___
🗺️ **자동 스테이지 순환 & 파밍 모드**
* 일반 모드
  * 몬스터 처치 수 달성 시 다음 스테이지 자동 진입
* 파밍 모드
  * 이미 클리어한 스테이지는 무한 리스폰
  * 지속적인 재화 수급 가능
* 이벤트 기반 구조
  * OnMonsterKilled 이벤트로 진행도 동기화
___
🧭 **맵 선택 & 스테이지 전환 연출**
* Clear한 스테이지 자유 이동
* 파밍/일반 모드 자동 분기
* Fade In/Out 기반 전환 연출
* Scene 전환 없이 StageData SO 교체 방식
___
🎁 **아이템 드랍 & 데이터 설계**
* ID 매핑 기반 관계형 데이터 구조
  * StageData → MonsterID 목록
  * MonsterData → DropTable 보유
* 몬스터 사망 시 확률 기반 아이템 드랍 처리
___
🛡️ **장비 & 강화 시스템**
* 장착 슬롯 강화 방식
  * 장비 교체 후에도 강화 수치 유지
* 중복 장비 자동 환전 → 강화 재료로 재사용 
* [파밍 → 중복 획득 → 환전 → 강화 → 성장] 순환 구조 설계
___
🎰 **가중치 확률 기반 뽑기 시스템**
* 누적 확률 방식 사용
* 뽑기 레벨 상승 시 상위 등급 확률 증가
* 중복 장신구 자동 환전
___
📊 **StatComponent 기반 스탯 모듈화**
* STR / DEX / INT / LUCK 구조 설계
* 성장 경로(레벨업, 장비, 슬롯 강화) 통합 관리
___
📜 **이벤트 기반 퀘스트 시스템**
* QuestManager가 외부 시스템 이벤트 구독
* 메인 퀘스트 자동 진행
* 반복 퀘스트 초과 달성 보상 처리
___
📱 **반응형 UI & 카메라 시스템**
* SafeArea 대응 (노치 / 펀치홀) 👉 [SafeArea.cs](https://github.com/KimJinSu-Git/Unity_Idle_Project/blob/main/Assets/3.Scripts/HummingBird/Camera/SafeArea.cs#L30)
* 기기 해상도에 따른 GridLayout 자동 계산 👉 [UI_ResponsiveGrid.cs](https://github.com/KimJinSu-Git/Unity_Idle_Project/blob/main/Assets/3.Scripts/HummingBird/UI/UI_ResponsiveGrid.cs#L39)
* Viewport 기반 카메라 추적
* UI 상태에 따른 Camera Offset 보정 👉 [PlayerTargetCamera.cs](https://github.com/KimJinSu-Git/Unity_Idle_Project/blob/main/Assets/3.Scripts/HummingBird/Visual/PlayerTargetCamera.cs#L50)
___
💤 **오프라인 방치 보상**
* 종료 시점과 재접속 시간 차 계산
* 최대 방치 시간 제한
* 실제 전투와 유사한 확률 기반 보상 시뮬레이션
___
💾 **비동기 데이터 저장 & 안정성**
* BinaryFormatter 기반 직렬화
* 비동기 File I/O
___
* 🎥 플레이 영상 :
* 📄 기술 문서 PDF :















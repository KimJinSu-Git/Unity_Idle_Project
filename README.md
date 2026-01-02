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
  <img src="https://github.com/KimJinSu-Git/3D_Personal_Project/blob/main/3D_Project_RPG/Assets/Screenshots/Image1.PNG" width="390"/> &nbsp; &nbsp;
  <img src="https://github.com/KimJinSu-Git/3D_Personal_Project/blob/main/3D_Project_RPG/Assets/Screenshots/Image2.PNG" width="390"/>
</p>
___
# 🔧 주요 구현 시스템
🧩 **전체 시스템 구조**
* **GameManager**
  * 게임 전체 흐름 오케스트레이션
  * 앱 생명주기 기반 자동 저장 👉 [스크립트 이름](스크립트 주소)
  * 데이터 리셋 시 예외 처리
* **DataManager**
  * 비동기 저장 / 로드
  * 로드 실패 시 안전한 신규 데이터 생성
* **StageManager**
  * 스테이지 진행 관리
  * Addressables 기반 몬스터 / 배경 로드

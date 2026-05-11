[System.Serializable]
//무기가 여러종류일때 공용으로 사용하는 변수들은 구조체로 묶어서 정의할시
// 변수가 추가되거나 삭제될때 구조체에 선언하기 떄문에 관리가 용이함

public class WeaponSettings
{
    public float attackRate;
    public float attackDistance;
    public bool isAutomaticAttack; //연속발사 여부
}

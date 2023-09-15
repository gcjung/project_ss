public enum PlayerState
{
    Idle,
    Moving,
    Attacking,
    Dead
}

public enum MonsterState
{
    Idle,
    Moving,
    Attacking,
    Dead
}

public enum StageState
{
    NormalWave,
    BossRoom,
    InfinityWave
}

public enum ItemType
{
    Weapon,
    Armor,
    Ring,
    Rune
}
public enum ItemGrade
{
    D,
    C,
    B,
    A,
    S
}
public enum GachaType
{
    None = -1,
    Item,
    Skill
}
public enum GachaGrade
{
    None = -1,
    Common,
    Uncommon,
    Rare,
    Epic,
    Regendary,
    Mythic,
}

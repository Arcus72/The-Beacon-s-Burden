using UnityEngine;

// Shared contract for every monster (Tank, Spider, Healer, ...).
// Extends IDamageable so weapons can damage monsters, while keeping the
// player and lighthouse (which are IDamageable but NOT monsters) out of
// the line of fire.
public interface IMonster : IDamageable
{
    GameObject[] Targets { get; set; }
    int SpawnMultiplier { get; }
    float SpawningChance { get; }
    void Heal(float amount);
}

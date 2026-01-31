using UnityEngine;

public class LevelSettings : MonoBehaviour
{
    [Header("Health drain/regen speed")]
    [Min(0.1f)] public float drainMultiplier = 1f;   // 1 = normal, 1.5 = 50% más rápido, 0.7 = más lento
}

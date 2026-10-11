using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(fileName = "CurrentArea", menuName = "Game/Current Area")]
public class CurrentAreaData : ScriptableObject
{
    [Header("Area actual")]
    public string areaKey;

    public UnityAction OnAreaChanged;

    public void SetCurrentArea(string newAreaKey)
    {
        areaKey = newAreaKey;
        OnAreaChanged?.Invoke();
    }
}

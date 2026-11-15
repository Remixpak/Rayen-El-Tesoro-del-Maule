using UnityEngine;
public class RegisterPlayer: MonoBehaviour
{
    private void Awake()
{
    if (GameManager.Instance != null)
    {
        GameManager.Instance.RegisterPlayer(gameObject);
    }
}
}

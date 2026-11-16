using UnityEngine;

public enum tipoCria
{
    conejo,
    gato,
    perro
}
public class CriaPihuchen: MonoBehaviour
{
    public tipoCria tipo;
    void OnDestroy()
    {
        GameManager.Instance.UpdateCriasPihuchen();
        switch(tipo)
        {
            case tipoCria.conejo:
                GameManager.Instance.cria1Destruida = true;
                break;
            case tipoCria.gato:
                GameManager.Instance.cria2Destruida = true;
                break;
            case tipoCria.perro:
                GameManager.Instance.cria3Destruida = true;
                break;
        }
    }
}
using UnityEngine;

public class IdentifiableObject : MonoBehaviour, IIdentifiable
{
    [SerializeField]
    int guid;

    public int GUID => guid;
}

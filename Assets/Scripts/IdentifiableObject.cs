using UnityEngine;

public class IdentifiableObject : MonoBehaviour
{
    [SerializeField]
    int guid;

    public int GUID => guid;
}

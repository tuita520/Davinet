using UnityEngine;

namespace Davinet
{
    public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {

        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (T)GameObject.FindObjectOfType(typeof(T));
                }

                return _instance;
            }
        }
    }
}

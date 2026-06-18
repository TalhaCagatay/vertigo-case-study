using UnityEngine;

namespace com.core.ui
{
    public class UIParent : MonoBehaviour
    {
        [SerializeField] private Transform  screensParent;
        [SerializeField] private Transform  popupsParent;
        [SerializeField] private GameObject backgroundGO;

        public Transform  ScreenParent => screensParent;
        public Transform  PopupParent  => popupsParent;
        public GameObject BackgroundGO => backgroundGO;
    }
}
using System.Collections.Generic;
using System.Linq;
using Reflex.Attributes;
using UnityEngine;

namespace com.core
{
    public class ControllerInitializer : MonoBehaviour
    {
        [Inject] private IEnumerable<IController> _controllers;

        private void Awake()
        {
            _controllers = _controllers.OrderBy(controller => controller.Order);
            foreach (var controller in _controllers) controller.Initialize();
        }
    }
}
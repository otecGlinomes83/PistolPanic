using PistolPanic.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PistolPanic.Presentation
{
    [DefaultExecutionOrder(-100)]
    public sealed class InputReader : MonoBehaviour, IPlayerInput
    {
        private bool _isPressed;

        public bool ConsumePress()
        {
            if (_isPressed == false)
            {
                return false;
            }

            _isPressed = false;

            return true;
        }

        private void Update()
        {
            Pointer currentPointer = Pointer.current;

            if (currentPointer == null)
            {
                return;
            }

            if (currentPointer.press.wasPressedThisFrame)
            {
                _isPressed = true;
            }
        }
    }
}

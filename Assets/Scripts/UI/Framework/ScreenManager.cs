// =============================================================================
// FILE: Assets/Scripts/UI/Framework/ScreenManager.cs
// OWNER ROLE: Client Programmer - UI
// RESPONSIBILITY: Screen stack (push/pop). Progressive disclosure: only unlocked screens are reachable (GDD 4.6).
// WORK ON: Unlock rules per feature, Android back button, fade transitions.
// STATUS: Implemented
// =============================================================================
using System.Collections.Generic;
using UnityEngine;

namespace SandsOfLegends.UI
{
    public sealed class ScreenManager : MonoBehaviour
    {
        private readonly Stack<UIScreen> _stack = new Stack<UIScreen>();

        public void Push(UIScreen screen)
        {
            if (_stack.Count > 0) _stack.Peek().OnHide();
            _stack.Push(screen);
            screen.OnShow();
        }

        public void Pop()
        {
            if (_stack.Count == 0) return;
            _stack.Pop().OnHide();
            if (_stack.Count > 0) _stack.Peek().OnShow();
        }
    }
}

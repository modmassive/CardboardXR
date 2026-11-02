using System;

namespace CardboardXR.Stereo
{
    // Bridges the phone-screen menu and CoreServices without creating
    // an assembly dependency from Stereo to Demos.
    public static class GameModeHub
    {
        public static event Action ToggleRequested;
        public static event Action<bool> StateChanged;

        public static bool IsActive { get; private set; }

        public static void Toggle()
        {
            ToggleRequested?.Invoke();
        }

        public static void SetActive(bool active)
        {
            if (IsActive == active)
                return;

            IsActive = active;
            StateChanged?.Invoke(active);
        }
    }
}

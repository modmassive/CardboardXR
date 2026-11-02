using CardboardXR.Hands;
using CardboardXR.Interaction;
using CardboardXR.Stereo;
using UnityEngine;

namespace CardboardXR.Demos
{
    // The Core scene's shared pieces, for demo scenes to reach.
    public class CoreServices : MonoBehaviour
    {
        [SerializeField] HeadPoseFusion m_Fusion;
        [SerializeField] StereoRig m_Rig;
        [SerializeField] PassthroughController m_Passthrough;
        [SerializeField] RoomBoundary m_Boundary;
        [SerializeField] AnchorPlacer m_Placer;
        [SerializeField] GazePointer m_Gaze;
        [SerializeField] HandTracker m_HandTracker;
        [SerializeField] PinchInput m_PinchInput;
        [SerializeField] HandInteractorRig m_HandRig;
        [SerializeField] DemoManager m_Demos;
        [SerializeField] SettingsSummoner m_Settings;

        bool m_GameMode;
        CoreState m_BeforeGameMode;
        bool m_PinchWasOn;

        public static CoreServices Instance { get; private set; }

        public HeadPoseFusion Fusion => m_Fusion;
        public StereoRig Rig => m_Rig;
        public PassthroughController Passthrough => m_Passthrough;
        public RoomBoundary Boundary => m_Boundary;
        public AnchorPlacer Placer => m_Placer;
        public GazePointer Gaze => m_Gaze;
        public HandTracker HandTracker => m_HandTracker;
        public PinchInput PinchInput => m_PinchInput;
        public HandInteractorRig HandRig => m_HandRig;
        public DemoManager Demos => m_Demos;
        public SettingsSummoner Settings => m_Settings;
        public Transform Head => m_Fusion.transform;

        public CoreState State => new(
            m_Fusion.CurrentMode == HeadPoseFusion.Mode.SixDof,
            m_Passthrough.IsOn,
            m_HandTracker.enabled);

        public bool IsGameMode => m_GameMode;

        void Awake()
        {
            Instance = this;
            GameModeHub.ToggleRequested += ToggleGameMode;
        }

        void OnDestroy()
        {
            GameModeHub.ToggleRequested -= ToggleGameMode;

            if (Instance == this)
                Instance = null;
        }

        void ToggleGameMode()
        {
            if (m_GameMode)
                DisableGameMode();
            else
                EnableGameMode();
        }

        void EnableGameMode()
        {
            if (m_GameMode)
                return;

            // Save the user's current configuration before applying the preset.
            m_BeforeGameMode = State;
            m_PinchWasOn = m_PinchInput.IsOn;
            m_GameMode = true;

            m_PinchInput.SetOn(false);
            Apply(new CoreState(false, false, false));

            GameModeHub.SetActive(true);

            Debug.Log("[Game Mode] Enabled: 3DoF, passthrough off, hands off, pinch off");
        }

        void DisableGameMode()
        {
            if (!m_GameMode)
                return;

            // Restore the saved state, respecting the requirements of the scene
            // that is currently active.
            var restore = m_BeforeGameMode;

            if (m_Demos.Current != null)
            {
                restore = CoreState.Resolve(restore, m_Demos.Current.Requirements);
            }
            else if (m_Demos.Catalog != null)
            {
                restore = CoreState.Resolve(restore, m_Demos.Catalog.LobbyRequirements);
            }

            m_GameMode = false;

            m_PinchInput.SetOn(m_PinchWasOn);
            Apply(restore);

            GameModeHub.SetActive(false);

            Debug.Log($"[Game Mode] Disabled: restored {restore}, pinch {(m_PinchWasOn ? "on" : "off")}");
        }

        // Switches to the state in an order that holds: hands first, then
        // 6DoF, then passthrough. Game Mode clamps every request from demos.
        public void Apply(CoreState state)
        {
            if (m_GameMode)
                state = new CoreState(false, false, false);

            m_HandTracker.enabled = state.Hands;

            if (!state.Passthrough)
                m_Passthrough.Choose(false);

            m_Fusion.SetMode(
                state.SixDof
                    ? HeadPoseFusion.Mode.SixDof
                    : HeadPoseFusion.Mode.ThreeDof);

            if (state.Passthrough)
                m_Passthrough.Choose(true);
        }
    }
}

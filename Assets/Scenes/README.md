# Scenes

The MVP runtime uses two scenes and one in-scene battle presentation:

1. Open Unity 2022.3.62f3.
2. Run **Sands → Setup MVP Home Scene**. The installer creates/repairs Boot.unity and Home.unity and registers them in that order.
3. Boot contains the single persistent GameBootstrap. It loads/creates GameState, refreshes energy and daily quests, then loads Home.
4. Home contains HomeScreen. GameBootstrap binds GameFlowController after Home loads.
5. Campaign opens the Battle presentation inside Home. BattleView publishes BattleEvents; the result panel offers Return to City or the next stage. Return to City restores the Home world.

The Android build menu ensures Boot is first and Home is second before invoking BuildPlayer. A separate Battle.unity scene is intentionally not introduced in this task.

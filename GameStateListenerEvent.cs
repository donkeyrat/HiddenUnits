using UnityEngine;
using UnityEngine.Events;
using Landfall.TABS.GameState;

namespace HiddenUnits {
    
    public class GameStateListenerEvent : GameStateListener {
        
        public override void OnEnterBattleState() { onEnterBattleEvent.Invoke(); }

        public override void OnEnterPlacementState() { onEnterPlacementEvent.Invoke(); }

        public UnityEvent onEnterBattleEvent = new UnityEvent();

        public UnityEvent onEnterPlacementEvent = new UnityEvent();
    }
}

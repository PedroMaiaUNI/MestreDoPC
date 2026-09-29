using System;

namespace MestreDoPC
{
    public enum GameState { Menu, Playing, Paused, Evaluation }

    public class GameManager : Singleton<GameManager>
    {
        public GameState State { get; private set; } = GameState.Menu;
        public GameMode CurrentMode { get; set; } = GameMode.Assembly;
        public Difficulty CurrentDifficulty { get; set; } = Difficulty.Easy;

        public event Action<GameState> OnStateChanged;

        public void SetState(GameState newState)
        {
            if (State == newState) return;
            State = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}

using UnityEngine;

// Switches background music with the game state:
// title music is handled by OpeningDirector, the ending by EndingDirector;
// this starts the per-stage tune during play and silences it on results.
public class MusicDirector : MonoBehaviour
{
    bool stagePlaying;

    void Update()
    {
        if (EndingDirector.IsActive)
        {
            stagePlaying = false;
            return;
        }

        if (!GameSession.HasStarted)
        {
            stagePlaying = false;
            return;
        }

        if (GameSession.HasEnded)
        {
            if (stagePlaying)
            {
                stagePlaying = false;
                RetroSfx.StopMusic();
            }
            return;
        }

        if (!stagePlaying)
        {
            stagePlaying = true;
            RetroSfx.PlayStageMusic(LevelManager.CurrentStage);
        }
    }
}

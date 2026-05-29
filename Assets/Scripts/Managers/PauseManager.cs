using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : Singleton<PauseManager>
{
    private bool isPaused = false;

    public void TogglePauseMenu(bool newState)
    {
        isPaused = newState;
        
        // 1. Notificar al sistema de interfaz de usuario
        UIMenuManager.Instance.ToggleMenu(newState);
   
        // 2. Obtener los jugadores activos desde el GameManager para cambiar sus controles
        var activePlayers = GameManager.Instance.GetActivePlayerControllers();
        if (activePlayers == null) return;

        foreach (var player in activePlayers)
        {
            if (player == null) continue;

            if (newState)
            {
                // Si el menú de pausa está encendido -> Controles de Menú
                player.EnablePauseMenuControls();
            }
            else
            {
                // Si el menú de pausa está apagado -> Controles de Gameplay
                player.EnableGameplayControls();
            }
        }
    }

    // Método de utilidad por si necesitas consultar desde otros scripts si el juego está pausado
    public bool IsPaused() => isPaused;
}
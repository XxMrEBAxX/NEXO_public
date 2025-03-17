using UnityEngine;

namespace BirdCase
{
    public class DenyJumpClashPos : MonoBehaviour
    {
        private PlayerBase player1;
        private PlayerBase player2;
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (other.TryGetComponent(out PlayerBase player))
                {
                    if (player.PlayerType == PlayerType.LASER)
                    {
                        player1 = player;
                    }
                    else
                    {
                        player2 = player;
                    }
                    player.PlayerJumpControlClientRPC(false);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (other.TryGetComponent(out PlayerBase player))
                {
                    if (player.PlayerType == PlayerType.LASER)
                    {
                        player1 = null;
                    }
                    else
                    {
                        player2 = null;
                    }
                    player.PlayerJumpControlClientRPC(true);
                }
            }
        }

        private void OnDisable()
        {
            if (player1 != null)
            {
                player1.PlayerJumpControlClientRPC(true);
            }
            if (player2 != null)
            {
                player2.PlayerJumpControlClientRPC(true);
            }
        }
    }
}

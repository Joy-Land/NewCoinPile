using UnityEngine;

namespace CoinPileScript
{
    public partial class CoinPile
    {
        void OnDrawGizmos()
        {
            // Draw a semitransparent red cube at the transforms position
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
        }
    }
}

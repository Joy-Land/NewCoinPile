using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Item
{
    public class CoinRope : MonoBehaviour
    {
        [SerializeField] private GogoGaga.OptimizedRopesAndCables.Rope rope;

        public void SetRopePosition(Transform startPoint, Transform endPosition)
        {
            rope.startPoint = startPoint;
            rope.endPoint = endPosition;
        }

        public void SetRopeLength(float length)
        {
            rope.ropeLength = length;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Item
{
    public class CoinIce : MonoBehaviour
    {
        [SerializeField] private GameObject ice1;
        [SerializeField] private GameObject ice2;
        [SerializeField] private GameObject ice3;

        public void SetActiveStatus(int status)
        {
            if (status <= 0)
            {
                if (ice1 != null)
                {
                    ice1.SetActive(false);
                }
                if (ice2 != null)
                {
                    ice2.SetActive(false);
                }
                if (ice3 != null)
                {
                    ice3.SetActive(false);
                }
            }
            else if (status == 1)
            {
                if (ice1 != null)
                {
                    ice1.SetActive(true);
                }
                if (ice2 != null)
                {
                    ice2.SetActive(false);
                }
                if (ice3 != null)
                {
                    ice3.SetActive(false);
                }
            }
            else if (status == 2)
            {
                if (ice1 != null)
                {
                    ice1.SetActive(true);
                }
                if (ice2 != null)
                {
                    ice2.SetActive(true);
                }
                if (ice3 != null)
                {
                    ice3.SetActive(false);
                }
            }
            else
            {
                if (ice1 != null)
                {
                    ice1.SetActive(true);
                }
                if (ice2 != null)
                {
                    ice2.SetActive(true);
                }
                if (ice3 != null)
                {
                    ice3.SetActive(true);
                }
            }
        }
    }
}

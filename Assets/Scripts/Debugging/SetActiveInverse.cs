using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetActiveInverse : MonoBehaviour
{
    [SerializeField] GameObject opp;
    private void OnEnable() {
        opp.SetActive(false);
    }
    
}

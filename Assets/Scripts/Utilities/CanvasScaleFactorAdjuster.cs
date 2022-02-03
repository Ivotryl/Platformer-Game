using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
 
public class CanvasScaleFactorAdjuster : MonoBehaviour {
    [SerializeField] private Camera MainCamera;

    private void Awake() {
        MainCamera = Camera.main;
    }
 
    private void Start(){
        AdjustScalingFactor();
    }
 
    private void LateUpdate() {
        AdjustScalingFactor();
    }
 
    private void AdjustScalingFactor() {
        GetComponent<CanvasScaler>().scaleFactor = MainCamera.GetComponent<PixelPerfectCamera>().pixelRatio;
    }
}
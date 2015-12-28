﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ResourceManager : MonoBehaviour {

    Text resourceName; //Name of the Resource. Should be only 2 letters.
    Image underBar; //Bar that represents max resource
    Image overBar; //Bar that represents current resource
    Text fraction; //Text on bar that describes the amount

    // Use this for initialization
    void Start() {
        resourceName = gameObject.GetComponentsInChildren<Text>()[0];
        underBar = gameObject.GetComponentsInChildren<Image>()[0];
        overBar = gameObject.GetComponentsInChildren<Image>()[1];
        fraction = gameObject.GetComponentsInChildren<Text>()[1];
    }

    // Update is called once per frame
    void Update() {

    }

    public void setResourceName(string name) {
        resourceName.text = name;
    }

    public void setUnderBarColor(Color color) {
        underBar.color = color;
    }

    public void setOverBarColor(Color color) {
        overBar.color = color;
    }

    /**
     * Scale should be in the range [0, 1]
     */
    public void setBarScale(float scale) {
        Vector3 v = overBar.gameObject.GetComponent<RectTransform>().localScale;
        v.x = scale;
        v = overBar.gameObject.GetComponent<RectTransform>().localScale;
    }

    public void setFraction(int numerator, int denominator) {
        fraction.text = string.Format("{0}/{0}", numerator, denominator);
    }

    public void setFractionString(string s) {
        fraction.text = s;
    }
}

﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System;

/**
 * This class manages a TextBox prefab
 */
public class TextBoxView : PooledBehaviour {

    [SerializeField]
    private Text text;
    [SerializeField]
    private Image background;
    [SerializeField]
    private Outline outline;

    private float TextAlpha { set { Util.SetTextAlpha(text, value); } }

    private float BackgroundAlpha {
        set {
            Util.SetImageAlpha(background, value);
        }
    }

    private float OutlineAlpha {
        set {
            Util.SetOutlineAlpha(outline, value);
        }
    }

    public const int BLIP_INTERVAL = 1; //Letters needed for a sound to occur
    public const int CHARS_PER_LINE = 44; //Needed for word wrapping function
    public const float FADE_DURATION = 5f;

    public virtual void WriteText(TextBox textBox, Action callBack = null) {
        StartCoroutine(TypeWriter(text, textBox, callBack));
    }

    IEnumerator TypeWriter(Text text, TextBox textBox, Action callBack) {
        text.color = textBox.Color;

        switch (textBox.Effect) {
            case TextEffect.OLD:
                text.text = textBox.RawText;
                break;
            case TextEffect.FADE_IN:
                text.text = textBox.RawText;
                Color color = text.color;
                color.a = 0;
                text.color = color;
                while (text.color.a < 1) {
                    Color c = text.color;
                    c.a += Time.deltaTime * 3;
                    text.color = c;
                    yield return null;
                }
                break;
            case TextEffect.TYPE:
                string[] currentTextArray = new string[textBox.TextArray.Length];
                bool[] taggedText = new bool[currentTextArray.Length];

                //Prescreen and enable all html tags and spaces
                for (int i = 0; i < currentTextArray.Length; i++) {
                    if (Regex.IsMatch(textBox.TextArray[i], "(<.*?>)")) {
                        currentTextArray[i] = textBox.TextArray[i];
                        taggedText[i] = true;
                    }
                }
                float timer = 0;
                int index = 0;
                int soundCount = 0;
                text.text = string.Join("", currentTextArray);
                string wrapper = "\u2007";
                while (index < textBox.TextArray.Length) {
                    if ((timer += Time.deltaTime) >= textBox.TimePerLetter) {
                        if (!taggedText[index]) {

                            //Preset forward characters to ensure wrapping
                            if (!string.Equals(currentTextArray[index], wrapper)) {
                                int start = index;

                                //Don't overshoot, only make one word, and don't replace any tagged text
                                while (start < textBox.TextArray.Length && !textBox.TextArray[start].Equals(" ") && !taggedText[start]) {
                                    currentTextArray[start++] = wrapper;
                                }
                            }
                            currentTextArray[index] = textBox.TextArray[index];

                            //Don't reset timer or make sound on spaces
                            if (!Regex.IsMatch(textBox.TextArray[index], " ")) {
                                timer = 0;
                                if (--soundCount <= 0) {
                                    soundCount = BLIP_INTERVAL;
                                    Game.Instance.Sound.Play(textBox.SoundLoc);
                                }
                            }
                        }
                        text.text = string.Join("", currentTextArray);
                        index++;
                    }
                    yield return null;
                }
                break;
        }
        textBox.IsDone = true;
        if (callBack != null) {
            callBack.Invoke();
        }

        float duration = textBox.Duration;
        if (duration > 0) {
            while ((duration -= Time.deltaTime) > 0) {
                yield return null;
            }
            float timer = 0;
            while ((timer += Time.deltaTime) < FADE_DURATION) {
                TextAlpha = Mathf.Lerp(1, 0, timer / FADE_DURATION);
                BackgroundAlpha = Mathf.Lerp(1, 0, timer / FADE_DURATION);
                yield return null;
            }
            ObjectPoolManager.Instance.Return(this);
        }
    }

    public override void Reset() {
        text.text = "";
        text.color = Color.white;
        background.color = Color.black;
        outline.effectColor = Color.white;
    }
}
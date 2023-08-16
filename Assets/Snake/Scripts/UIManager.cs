using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;
using DG.Tweening;
using System;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    [SerializeField] Button playButton;
    [SerializeField] ButtonState leftButton;
    [SerializeField] ButtonState rightButton;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI gameOverText;


    Player player => Player.instance;

    public static UIManager instance {
        get; private set;
    }

    public event Action PlayClicked;

    void Awake() {
        instance = this;

        playButton.onClick.AddListener(() => PlayClicked?.Invoke());
    }

    public void SetScore(int score) {
        scoreText.text = $"Score: {score}";

        if (score > 0) {
            scoreText.rectTransform.DOScale(1.1f, 0.2f)
                .SetLoops(2, LoopType.Yoyo);
        }
    }

    public bool IsRightButtonPressed => rightButton.IsPressed;
    public bool IsLeftButtonPressed => leftButton.IsPressed;

    public void SetPlayButton(bool visible) {
        SetVisible(playButton, visible, -500f, 0);
        playButton.enabled = visible;
    }

    public void SetGameOverText(bool visible) {
        SetVisible(gameOverText, visible, 500f, 0);
    }

    void SetVisible(UIBehaviour behaviour, bool visible, float hiddenY, float visibleY) {
        behaviour.GetComponent<RectTransform>()
            .DOAnchorPosY(visible ? visibleY : hiddenY, 0.5f)
            .SetEase(visible ? Ease.OutBack : Ease.Linear);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Canvas canvas;
    public RawImage rawImage;
    public SpriteRenderer spriteRenderer;
    private void Start()
    {
        var _rawImage = Instantiate(rawImage, canvas.transform);
        var _sprite = Instantiate(spriteRenderer, transform);

        var playerPref = Resources.Load<GameObject>($"Player/Farmer");
        var _playerPref = Instantiate(playerPref, transform);
        Util.ChangeLayer(_playerPref, 6);
    }
}

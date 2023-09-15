using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Canvas canvas;

    private void Start()
    {
        var playerPref = Resources.Load<GameObject>($"Player/Farmer");
        var _playerPref = Instantiate(playerPref, transform);
        //Util.ChangeLayer(_playerPref, 6);
        _playerPref.transform.localScale = new Vector3(5, 5, 1);
    }
}

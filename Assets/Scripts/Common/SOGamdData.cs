using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Data/GameData")]
//이름은 걍 ScriptableObject 줄여서  SO 붙임
public class SOGamdData : ScriptableObject
{
    [Serializable]
    public class GameData
    {
        public string name;
        public string value;
        public string description;
    }

    public List<GameData> gameDataList = new List<GameData>();

    /// <summary>
    /// 인스펙터에서 데이터를 쉽게 변경할 수 있도록 함.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public T GetGameDataName<T>(string name)
    {
        GameData gameData = gameDataList.Find((data) => data.name.Equals(name));

        if (gameData == null)
        {
            Debug.Log($"{name}으로 찾을 수 있는 데이터가 없음.");
        }

        if (string.IsNullOrEmpty(gameData.value))
        {
            Debug.Log($"{name}은 존재하지만, 값이 없음");
        }

        object obj = null;

        if (typeof(T) == typeof(int))
        {
            if (int.TryParse(gameData.value, out int valueInt))
            {
                obj = valueInt;
            }
            else
            {
                Debug.Log($"{gameData.value}를 int로 변환할 수 없음.");
            }
        }

        else if (typeof(T) == typeof(float))
        {
            if (float.TryParse(gameData.value, out float valueFloat))
            {
                obj = valueFloat;
            }
            else
            {
                Debug.Log($"{gameData.value}를 float로 변환할 수 없음.");
            }
        }

        else
        {
            Debug.Log($"{typeof(T).Name} 지원 x");
        }

        T finalObj = (T)obj;
        return finalObj;
    }
}

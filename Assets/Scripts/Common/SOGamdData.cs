using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Data/GameData")]
//�̸��� �� ScriptableObject �ٿ���  SO ����
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
    /// �ν����Ϳ��� �����͸� ���� ������ �� �ֵ��� ��.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public T GetGameDataName<T>(string name)
    {
        GameData gameData = gameDataList.Find((data) => data.name.Equals(name));

        if (gameData == null)
        {
            Debug.Log($"{name}���� ã�� �� �ִ� �����Ͱ� ����.");
        }

        if (string.IsNullOrEmpty(gameData.value))
        {
            Debug.Log($"{name}�� ����������, ���� ����");
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
                Debug.Log($"{gameData.value}�� int�� ��ȯ�� �� ����.");
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
                Debug.Log($"{gameData.value}�� float�� ��ȯ�� �� ����.");
            }
        }

        else
        {
            Debug.Log($"{typeof(T).Name} ���� x");
        }

        T finalObj = (T)obj;
        return finalObj;
    }
}

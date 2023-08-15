using UnityEngine;
using UnityEngine.UI;

public class ItemPanel : PanelBase
{
    [SerializeField] private Transform buttonTrasform;
    [SerializeField] private Transform goTrasform;
    [SerializeField] private WeaponeListPanel weaponeListPanel;
    private Button[] buttons;
    private GameObject[] gameObjects;

    public override void InitPanel()
    {
        SetObjects();
    }

    private void SetObjects()
    {
        int count = buttonTrasform.childCount;

        buttons = new Button[count];
        gameObjects = new GameObject[count];

        for (int i = 0; i < count; ++i)
        {
            buttons[i] = buttonTrasform.GetChild(i).GetComponent<Button>();
            gameObjects[i] = goTrasform.GetChild(i).gameObject;
        }
        SetButtonTest();
    }

    private void SetButtonTest()
    {
        for (int i = 0; i < buttons.Length; ++i)
        {
            int iIndex = i;

            buttons[iIndex].onClick.AddListener(() =>
            {
                for (int j = 0; j < gameObjects.Length; ++j)
                {
                    int jIndex = j;

                    if (iIndex == jIndex)
                    {
                        gameObjects[jIndex].SetActive(true);
                    }
                    else
                    {
                        gameObjects[jIndex].SetActive(false);
                    }
                }
            });
        }
    }

    public void GetItemData()
    {
        weaponeListPanel.GetWeaponeData();
    }

    public override void OpenPanel()
    {

    }

    public override void ClosePanel()
    {

    }
}

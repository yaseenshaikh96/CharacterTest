using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu_HandleUI : MonoBehaviour
{
    GameData_Script gameData;
    [SerializeField] private Image background_Img;
    [SerializeField] private TMP_Text gameTitle_Text;
    [SerializeField] private Image menuOptions_Cont;
    [SerializeField] private List<Button> menuOptionsList_But;
    private int screenWidth, screenHeight;

    void Start()
    {  
        gameData = GameData_Script.instance;
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        background_Img.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, screenWidth);
        background_Img.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, screenHeight);

        gameTitle_Text.rectTransform.anchoredPosition = new Vector2( 0, screenHeight * 0.8f );

        float menuWidth = screenWidth * 0.2f;
        float menuHeight = screenHeight * 0.6f;
        menuOptions_Cont.rectTransform.anchoredPosition = new Vector2(0, screenHeight * 0.7f);
        menuOptions_Cont.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,menuHeight);
        menuOptions_Cont.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,menuWidth);
        for(int i=0; i<menuOptionsList_But.Count; i++)
            menuOptionsList_But[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -i * menuHeight * 0.2f);
        menuOptionsList_But[0].onClick.AddListener(OnStartGameButtonPressed);
        menuOptionsList_But[1].onClick.AddListener(OnShaderButtonPressed);
        menuOptionsList_But[2].onClick.AddListener(OnPathfindingButtonPressed);
        menuOptionsList_But[3].onClick.AddListener(OnPlayerTestButtonPressed);
        
    }

    //----------------------------------------//

    public void OnStartGameButtonPressed() {
        if(!gameData.IsChangingScene)
            gameData.ChangeScene(SceneNameEnum.Game_01_Scene);
    }

    public void OnShaderButtonPressed() {
    if(!gameData.IsChangingScene)
        gameData.ChangeScene(SceneNameEnum.ShaderTest_Scene);
    }

    public void OnPathfindingButtonPressed() {
    if(!gameData.IsChangingScene)
        gameData.ChangeScene(SceneNameEnum.Pathfinding_Scene);
    }
    public void OnPlayerTestButtonPressed() {
    if(!gameData.IsChangingScene)
        gameData.ChangeScene(SceneNameEnum.PlayerTest_Scene);
    }

}

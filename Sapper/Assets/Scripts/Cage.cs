using UnityEngine;
using UnityEngine.UI;

public class Cage : MonoBehaviour
{
    public Image misstake;
    public Sprite[] sprites;
    [HideInInspector]
    public Image eventCage;
    public bool flag, open;
    public int number;//-1-мина
    public int x, y;
    public SapperManager manager;
    void Start()
    {
        eventCage = GetComponent<Image>();
    }
    /// <summary>
    /// поставить или убрать флаг
    /// </summary>
    public void OnChangeCountFlag()
    {
        if (open || manager.win || manager.gameover)
            return;
        if(!flag)
        {
            if (manager.mines - manager.flags == 0)
                return;
            eventCage.sprite = sprites[1];
            if (number != -1)
                manager.correctlyFlags++;
            manager.flags++;
        }
        else
        {
            eventCage.sprite = sprites[0];
            if(number != -1)
                manager.correctlyFlags--;
            manager.flags--;
        }
        manager.leftMinesText.text = "мин осталось: " + (manager.mines - manager.flags).ToString();
        flag = !flag;
    }
    /// <summary>
    /// проверить на наличие мины
    /// </summary>
    public void OnClick()
    {
        if (flag || manager.win || manager.gameover)
            return;
        if (!eventCage)//при нажатии на кнопку заново, чтобы не было ошибок из-за нажатой в это время клетки
            return;
        if (open)
        {
            int flagsAround = 0;
            for (int addX = -1; addX <= 1; addX++)
                for (int addY = -1; addY <= 1; addY++)
                    if (((y + addY) < manager.ySize && (x + addX) < manager.xSize) &&
                       ((y + addY) >= 0 && (x + addX) >= 0) &&
                        manager.cages[x + addX, y + addY].flag)
                        flagsAround++;
            if (flagsAround >= number)
                OpenCagesAround();
            return;
        }
        if(number != -1)
        {
            open = true;
            if (number == 0)
                OpenCagesAround();
            
            eventCage.sprite = sprites[number + 3];//смещение на 3 из-за такого расположения картинок
            manager.close--;
            manager.leftCloseCagesText.text = "пустых клеток закрыто: " + (manager.close - manager.mines);
            if (manager.close - manager.mines == 0)
            {
                manager.win = true;
                manager.winObj.SetActive(true);
            }
                
        }
        else
        {
            eventCage.sprite = sprites[2];
            misstake.enabled = true;
            manager.OnGameOver();
        }
    }
    /// <summary>
    /// сканирование в радиусе одной клетки
    /// </summary>
    void OpenCagesAround()
    {
        for (int addX = -1; addX <= 1; addX++)
            for (int addY = -1; addY <= 1; addY++)
                if (((y + addY) < manager.ySize && (x + addX) < manager.xSize) &&
                   ((y + addY) >= 0 && (x + addX) >= 0) &&
                    !manager.cages[x + addX, y + addY].open)
                    manager.cages[x + addX, y + addY].OnClick();
    }
}

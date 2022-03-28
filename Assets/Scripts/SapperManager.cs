using System;
using UnityEngine;
using UnityEngine.UI;
public class SapperManager : MonoBehaviour
{
    public Transform content;
    public float xScale, yScale;
    public int xSize = 50, ySize = 50;
    public GameObject cage;
    public Cage[,] cages;
    public int mines = 500, flags, correctlyFlags, close;
    public bool win, gameover;
    public GameObject winObj, gameoverObj;
    Vector3 ScreenPos, pos;
    public InputField fieldX, fieldY, fieldMines;
    DateTime start;
    public Text timeText, leftMinesText, leftCloseCagesText;
    public CameraMover mover;
    [Header("отступ границ поля")]
    public float indent;
    void OnEnable()
    {
        CreateSapper();
    }
    void OnDisable()
    {
        DestroySapper();
    }
    void OnGUI()
    {

        Camera c = Camera.main;
        Event e = Event.current;
        Vector2 mousePos = new Vector2();
        mousePos.x = e.mousePosition.x;
        mousePos.y = c.pixelHeight - e.mousePosition.y;
        ScreenPos = new Vector3(mousePos.x, mousePos.y, c.nearClipPlane);
        pos = c.ScreenToWorldPoint(ScreenPos);

    }
    /// <summary>
    /// Создание поля сапёра
    /// </summary>
    public void CreateSapper()
    {
        close = xSize * ySize;
        leftMinesText.text = "мин осталось: " + mines.ToString();
        leftCloseCagesText.text = "пустых клеток закрыто: " + (close - mines);
        start = DateTime.Now;
        winObj.SetActive(false);
        gameoverObj.SetActive(false);
        win = false;
        gameover = false;
        flags = 0;
        correctlyFlags = 0;

        mover.min.x = 0 - xScale / 2 - indent;
        mover.min.y = 0 - yScale / 2 - indent;

        mover.max.x = xSize * xScale - yScale / 2 + indent;
        mover.max.y = ySize * yScale - yScale / 2 + indent;

        mover.OnChangeCameraPosition();

        cages = new Cage[xSize, ySize];
        for (int y = 0; y < ySize; y++)
            for (int x = 0; x < xSize; x++)
            {
                cages[x,y] = Instantiate(cage, new Vector2(x * xScale, y * yScale), Quaternion.identity, content).GetComponent<Cage>();
                cages[x, y].manager = this;
                cages[x, y].x = x;
                cages[x, y].y = y;
            }
        int randX, randY;
        for (int i = 0; i < mines; i++)
        {
            randX = UnityEngine.Random.Range(0, xSize - 1);
            randY = UnityEngine.Random.Range(0, ySize - 1);
            for (int j = 0; j < ySize * xSize + 1; j++)
            {
                try
                {
                    if (cages[randX, randY].number != -1)
                    {
                        cages[randX, randY].number = -1;
                        break;
                    }
                }
                catch
                {
                    Debug.Log(randX + " " + randY + " " + cages.GetLength(0) + " " + cages.GetLength(1));
                }
                randX++;

                if (randX >= xSize - 1)
                { 
                    randX = 0;
                    randY++; 
                }
                if (randY >= ySize - 1)
                    randY = 0;
            }
        }
        for (int y = 0; y < ySize; y++)
            for (int x = 0; x < xSize; x++)
                if (cages[x, y].number != -1)
                    cages[x, y].number = SearchMine(x, y);
    }
    /// <summary>
    /// Уничтожение поля сапёра
    /// </summary>
    public void DestroySapper()
    {
        for (int y = 0; y < cages.GetLength(1); y++)
            for (int x = 0; x < cages.GetLength(0); x++)
                Destroy(cages[x, y].gameObject);
        cages = null;
    }
    /// <summary>
    /// проверяет строку на корректность вводимого значения(количества столбцов) и, в случае успеха, записывает
    /// </summary>
    /// <param name="xStr">проверяемая строка</param>
    public void ParseX(string xStr)
    {
        try
        {
            xSize = Convert.ToInt32(xStr);
            xSize = Mathf.Clamp(xSize, 1, 50);
        }
        catch
        {
            xSize = 10;
        }
        mines = Mathf.Clamp(mines, 1, ySize * xSize);
        fieldMines.text = mines.ToString();
        fieldX.text = xSize.ToString();
    }
    /// <summary>
    /// проверяет строку на корректность вводимого значения(количества строк) и, в случае успеха, записывает
    /// </summary>
    /// <param name="yStr">проверяемая строка</param>
    public void ParseY(string yStr)
    {
        try
        {
            ySize = Convert.ToInt32(yStr);
            ySize = Mathf.Clamp(ySize, 1, 50);
        }
        catch
        {
            ySize = 10;
        }
        mines = Mathf.Clamp(mines, 1, ySize * xSize);
        fieldMines.text = mines.ToString();
        fieldY.text = ySize.ToString();
    }
    /// <summary>
    /// проверяет строку на корректность вводимого значения(количества мин) и, в случае успеха, записывает
    /// </summary>
    /// <param name="minesStr">проверяемая строка</param>
    public void ParseMines(string minesStr)
    {
        try
        {
            mines = Convert.ToInt32(minesStr);
            mines = Mathf.Clamp(mines, 1, ySize * xSize);
        }
        catch
        {
            mines = Mathf.Clamp(25, 1, ySize * xSize);
        }
        fieldMines.text = mines.ToString();
    }
    /// <summary>
    /// считает мины вокруг клетки
    /// </summary>
    /// <param name="x">позиция x клетки</param>
    /// <param name="y">позиция y клетки</param>
    /// <returns>количесто мин вокруг клетки в позиции (x, y) в радиусе одной клетки</returns>
    int SearchMine(int x, int y)
    {
        int count = 0;
        for (int addX = -1; addX <= 1; addX++)
            for (int addY = -1; addY <= 1; addY++)
                if(((y + addY) < ySize && (x + addX) < xSize) &&
                   ((y + addY) >= 0 && (x + addX) >= 0))
                count = cages[x + addX, y + addY].number == -1?count+1 : count;
        return count;
    }
    /// <summary>
    /// Проигрыш
    /// </summary>
    public void OnGameOver()
    {
        gameover = true;
        gameoverObj.SetActive(true);

        for (int y = 0; y < ySize; y++)
            for (int x = 0; x < xSize; x++)
            {
                if(!cages[x, y].open && cages[x, y].number == -1 && !cages[x, y].misstake.enabled)
                cages[x, y].eventCage.sprite = cages[x, y].sprites[2];
            }
    }
    void Update()
    {
        if(!win && !gameover)
            timeText.text = (DateTime.Now - start).ToString();
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            Vector3 EndRay = Camera.main.transform.forward * 1000f;
            Vector3 StartRay = pos;
            Debug.DrawRay(StartRay, EndRay);
            RaycastHit2D h = Physics2D.Raycast(StartRay, EndRay);
            if (h && h.transform.GetComponent<Cage>())
            {
                Cage tempCage = h.transform.GetComponent<Cage>();
                //int x = tempCage.x;
                //int y = tempCage.y;
                tempCage.OnClick();
                
            }

        }
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            Vector3 EndRay = Camera.main.transform.forward * 1000f;
            Vector3 StartRay = pos;
            Debug.DrawRay(StartRay, EndRay);
            RaycastHit2D h = Physics2D.Raycast(StartRay, EndRay);
            if (h && h.transform.GetComponent<Cage>())
            {
                Cage tempCage = h.transform.GetComponent<Cage>();
                tempCage.OnChangeCountFlag();
            }
        }
    }
}

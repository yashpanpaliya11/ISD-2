using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CoinCatcher : MonoBehaviour
{
    const float Duration = 60f;
    float remaining = Duration, nextSpawn, nextBomb = 2.5f;
    int score, missed;
    bool finished;
    Transform crab;
    Text scoreLabel, timeLabel, resultLabel;
    Sprite coinSprite;

    void Start()
    {
        var camObject = new GameObject("Camera");
        var cam = camObject.AddComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 6;
        cam.backgroundColor = new Color(.035f, .16f, .24f); camObject.transform.position = new Vector3(0, 0, -10);
        var square = CreateSprite(false); coinSprite = CreateSprite(true);
        MakeShape("Water", square, new Vector2(0,0), new Vector2(15,10), new Color(.055f,.28f,.36f), -2);
        MakeShape("Sand", square, new Vector2(0,-4.7f), new Vector2(15, .65f), new Color(.9f,.72f,.43f), -1);
        var texture = Resources.Load<Texture2D>("Crab");
        var crabSprite = texture ? Sprite.Create(texture, new Rect(0,0,texture.width,texture.height), new Vector2(.5f,.5f), texture.width) : coinSprite;
        crab = MakeShape("Crab", crabSprite, new Vector2(0,-3.8f), Vector2.one * 1.65f, Color.white, 2).transform;
        BuildUI();
        nextSpawn = .4f;
    }
    Sprite CreateSprite(bool round)
    {
        const int n = 64; var tex = new Texture2D(n,n); tex.filterMode = FilterMode.Bilinear;
        for (int y=0; y<n; y++) for (int x=0; x<n; x++)
            tex.SetPixel(x,y,round && Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(32,32))>31.5f ? Color.clear : Color.white);
        tex.Apply(); return Sprite.Create(tex,new Rect(0,0,n,n),new Vector2(.5f,.5f),n);
    }
    GameObject MakeShape(string name, Sprite sprite, Vector2 position, Vector2 scale, Color color, int order)
    {
        var obj = new GameObject(name); obj.transform.position = position; obj.transform.localScale = scale;
        var renderer = obj.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.color = color; renderer.sortingOrder = order; return obj;
    }
    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.rKey.wasPressedThisFrame) { Restart(); return; }
        if (finished) return;
        remaining = Mathf.Max(0, remaining - Time.deltaTime);
        timeLabel.text = "TIME  " + Mathf.CeilToInt(remaining).ToString("00");
        if (remaining <= 0) { finished = true; resultLabel.text = "TIME'S UP!\n<size=28>Coins caught: " + score + "</size>"; return; }
        if (keyboard != null)
        {
            float direction = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1 : 0);
            var pos = crab.position; pos.x = Mathf.Clamp(pos.x + direction * 8f * Time.deltaTime, -6.55f, 6.55f); crab.position = pos;
        }
        nextSpawn -= Time.deltaTime;
        if (nextSpawn <= 0) { SpawnCoin(); nextSpawn = Random.Range(.55f, 1.05f); }
        nextBomb -= Time.deltaTime;
        if (nextBomb <= 0) { SpawnBomb(); nextBomb = Random.Range(2.5f, 4f); }
        foreach (var coin in FindObjectsByType<FallingCoin>(FindObjectsSortMode.None))
        {
            coin.transform.position += Vector3.down * coin.speed * Time.deltaTime;
            var pos = coin.transform.position;
            if (pos.y < -3.35f && pos.y > -4.35f && Mathf.Abs(pos.x - crab.position.x) < .8f)
            { score++; scoreLabel.text = "COINS  " + score; Destroy(coin.gameObject); }
            else if (pos.y < -5.1f) { missed++; Destroy(coin.gameObject); }
        }
        foreach (var bomb in FindObjectsByType<FallingBomb>(FindObjectsSortMode.None))
        {
            bomb.transform.position += Vector3.down * bomb.speed * Time.deltaTime;
            var pos = bomb.transform.position;
            if (pos.y < -3.35f && pos.y > -4.35f && Mathf.Abs(pos.x - crab.position.x) < .8f)
            {
                Destroy(bomb.gameObject); finished = true;
                resultLabel.text = "BOOM! GAME OVER\n<size=28>Coins caught: " + score + "</size>";
                return;
            }
            if (pos.y < -5.1f) Destroy(bomb.gameObject);
        }
    }
        
    void SpawnBomb()
    {
        var bomb = MakeShape("Bomb", coinSprite, new Vector2(Random.Range(-6.3f, 6.3f), 5.1f), Vector2.one * .7f, new Color(.95f, .12f, .12f), 1);
        bomb.AddComponent<FallingBomb>().speed = Random.Range(3.5f, 5f);
        var fuse = MakeShape("Fuse", coinSprite, Vector2.zero, Vector2.one * .22f, new Color(1f, .8f, .3f), 2);
        fuse.transform.SetParent(bomb.transform, false); fuse.transform.localPosition = new Vector3(0, .48f, 0);
    }
    void SpawnCoin()
    {
        var coin = MakeShape("Coin",coinSprite,new Vector2(Random.Range(-6.3f,6.3f),5.1f),Vector2.one*.5f,new Color(1f,.78f,.18f),1);
        coin.AddComponent<FallingCoin>().speed = Random.Range(3.2f,5.1f);
    }
    void Restart() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    void BuildUI()
    {
        var canvasObject = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280,720);
        if (!EventSystem.current) new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
        scoreLabel = Label(canvasObject.transform,"Score","COINS  0",26,TextAnchor.UpperLeft,new Vector2(30,-25),new Vector2(230,45));
        timeLabel = Label(canvasObject.transform,"Time","TIME  60",26,TextAnchor.UpperLeft,new Vector2(30,-65),new Vector2(230,45));
        resultLabel = Label(canvasObject.transform,"Result","",50,TextAnchor.MiddleCenter,Vector2.zero,new Vector2(700,180)); resultLabel.raycastTarget = false;
        var button = new GameObject("Restart Button",typeof(RectTransform),typeof(Image),typeof(Button)); button.transform.SetParent(canvasObject.transform,false);
        var rect = button.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(1,1); rect.pivot = new Vector2(1,1); rect.anchoredPosition = new Vector2(-30,-25); rect.sizeDelta = new Vector2(150,48);
        button.GetComponent<Image>().color = new Color(.12f,.39f,.45f); button.GetComponent<Button>().onClick.AddListener(Restart);
        Label(button.transform,"Label","RESTART",20,TextAnchor.MiddleCenter,Vector2.zero,new Vector2(150,48)).raycastTarget = false;
    }
    Text Label(Transform parent,string name,string text,int size,TextAnchor alignment,Vector2 offset,Vector2 dimensions)
    {
        var obj = new GameObject(name,typeof(RectTransform)); obj.transform.SetParent(parent,false); var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = alignment == TextAnchor.UpperLeft ? new Vector2(0,1) : new Vector2(.5f,.5f);
        rect.pivot = alignment == TextAnchor.UpperLeft ? new Vector2(0,1) : new Vector2(.5f,.5f); rect.anchoredPosition = offset; rect.sizeDelta = dimensions;
        var label = obj.AddComponent<Text>(); label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = size; label.alignment = alignment; label.color = Color.white; label.text = text; return label;
    }
}
public class FallingCoin : MonoBehaviour { public float speed; }

public class FallingBomb : MonoBehaviour { public float speed; }

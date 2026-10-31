using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BallGame : MonoBehaviour
{
    Rigidbody2D ball;
    Text messageText, timerText;
    float elapsed;
    int collected;
    const int Total = 8;
    Sprite circle, square;

    void Start()
    {
        circle = MakeSprite(true); square = MakeSprite(false);
        var cameraObject = new GameObject("Game Camera");
        var cam = cameraObject.AddComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 6.5f;
        cam.backgroundColor = new Color(0.055f, 0.08f, 0.15f); cameraObject.transform.position = new Vector3(0, 0, -10);
        var background = Shape("Arena", square, new Vector2(0, 0), new Vector2(15, 10), new Color(0.11f, 0.18f, 0.28f));
        background.GetComponent<SpriteRenderer>().sortingOrder = -2;
        Wall("Top", new Vector2(0, 5), new Vector2(15, .3f)); Wall("Bottom", new Vector2(0, -5), new Vector2(15, .3f));
        Wall("Left", new Vector2(-7.5f, 0), new Vector2(.3f, 10)); Wall("Right", new Vector2(7.5f, 0), new Vector2(.3f, 10));
        var crabTexture = Resources.Load<Texture2D>("Crab");
        var crabSprite = crabTexture != null ? Sprite.Create(crabTexture, new Rect(0, 0, crabTexture.width, crabTexture.height), new Vector2(.5f, .5f), crabTexture.width) : circle;
        var player = Shape("Player Ball", crabSprite, Vector2.zero, Vector2.one * 1.2f, Color.white);
        player.AddComponent<CircleCollider2D>(); ball = player.AddComponent<Rigidbody2D>(); ball.gravityScale = 0; ball.linearDamping = 4;
        Vector2[] positions = { new Vector2(-5.5f,-3.3f),new Vector2(-2.8f,-3.3f),new Vector2(2.8f,-3.3f),new Vector2(5.5f,-3.3f),new Vector2(-5.5f,3.3f),new Vector2(-2.8f,3.3f),new Vector2(2.8f,3.3f),new Vector2(5.5f,3.3f) };
        foreach (var p in positions) { var pickup = Shape("Pickup", circle, p, Vector2.one * .52f, new Color(1f,.78f,.25f)); var trigger = pickup.AddComponent<CircleCollider2D>(); trigger.isTrigger = true; pickup.AddComponent<BallPickup>().game = this; }
        BuildUI();
    }
    Sprite MakeSprite(bool round) {
        int n = 64; var texture = new Texture2D(n,n); texture.filterMode = FilterMode.Bilinear;
        for (int y=0;y<n;y++) for (int x=0;x<n;x++) { float d = Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(n/2f,n/2f)); texture.SetPixel(x,y,round && d>n/2f ? Color.clear : Color.white); }
        texture.Apply(); return Sprite.Create(texture,new Rect(0,0,n,n),new Vector2(.5f,.5f),n);
    }
    GameObject Shape(string name, Sprite sprite, Vector2 pos, Vector2 size, Color color) { var o = new GameObject(name); o.transform.position = pos; o.transform.localScale = size; var r=o.AddComponent<SpriteRenderer>(); r.sprite=sprite; r.color=color; return o; }
    void Wall(string name, Vector2 p, Vector2 size) { var o=Shape(name,square,p,size,new Color(.27f,.42f,.57f)); o.AddComponent<BoxCollider2D>(); }
    void FixedUpdate() {
        if (ball == null || collected >= Total || Keyboard.current == null) return;
        var k=Keyboard.current; float x=(k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0)-(k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0);
        float y=(k.wKey.isPressed || k.upArrowKey.isPressed ? 1 : 0)-(k.sKey.isPressed || k.downArrowKey.isPressed ? 1 : 0);
        ball.AddForce(new Vector2(x,y).normalized * 22f);
    }
    void Update() { if (collected < Total) { elapsed += Time.deltaTime; int seconds = Mathf.FloorToInt(elapsed); timerText.text = string.Format("TIME  {0:00}:{1:00}", seconds / 60, seconds % 60); } if (collected >= Total && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
    public void Collect() { collected++; if (collected >= Total) { ball.linearVelocity = Vector2.zero; messageText.text = "YOU WIN!\n<size=24>Press R to play again</size>"; } }
    void BuildUI() {
        var canvasObject = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas=canvasObject.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1280,720);
        if (UnityEngine.EventSystems.EventSystem.current == null) new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        var buttonObject = new GameObject("Restart Button", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
        buttonObject.transform.SetParent(canvasObject.transform, false);
        var buttonRect = buttonObject.GetComponent<RectTransform>(); buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(1, 1); buttonRect.pivot = new Vector2(1, 1); buttonRect.anchoredPosition = new Vector2(-35, -25); buttonRect.sizeDelta = new Vector2(150, 48);
        buttonObject.GetComponent<UnityEngine.UI.Image>().color = new Color(.2f, .35f, .48f);
        buttonObject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
        var buttonLabel = Label(buttonObject.transform, "Label", "RESTART", 20, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(150, 48)); buttonLabel.raycastTarget = false;
        timerText=Label(canvasObject.transform,"Timer","TIME  00:00",26,TextAnchor.UpperLeft,new Vector2(35,-30),new Vector2(300,60));
        messageText=Label(canvasObject.transform,"Win Message","",52,TextAnchor.MiddleCenter,Vector2.zero,new Vector2(700,180));
    }
    Text Label(Transform parent,string name,string value,int size,TextAnchor alignment,Vector2 offset,Vector2 dimensions) {
        var o=new GameObject(name,typeof(RectTransform)); o.transform.SetParent(parent,false); var rect=o.GetComponent<RectTransform>();
        if (alignment==TextAnchor.UpperLeft) rect.anchorMin=rect.anchorMax=new Vector2(0,1); else if(alignment==TextAnchor.UpperRight) rect.anchorMin=rect.anchorMax=new Vector2(1,1); else rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
        rect.pivot=alignment==TextAnchor.UpperLeft ? new Vector2(0,1) : alignment==TextAnchor.UpperRight ? new Vector2(1,1) : new Vector2(.5f,.5f); rect.anchoredPosition=offset; rect.sizeDelta=dimensions;
        var t=o.AddComponent<Text>(); t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.fontSize=size; t.alignment=alignment; t.color=Color.white; t.text=value; return t;
    }
}
public class BallPickup : MonoBehaviour { public BallGame game; void OnTriggerEnter2D(Collider2D other) { if (other.attachedRigidbody != null && other.gameObject.name == "Player Ball") { game.Collect(); Destroy(gameObject); } } }

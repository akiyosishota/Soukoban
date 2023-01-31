using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FieldArrayData : MonoBehaviour
{
    // タグリストの名前に紐づく番号
    private int _noBlock = 0;
    private int _staticBlock = 1;
    private int _moveBlock = 2;
    private int _player = 3;
    private int _target = 4;
    private bool _inputState = false;

    /// <summary>
    /// シーンに配置するオブジェクトのルートをヒエラルキーから設定する
    /// </summary>
    [Header("配置するオブジェクトの親オブジェクトを設定")]
    [SerializeField] private GameObject _fieldRootObject = default;
    [SerializeField] private GameObject _textUi = default;
    [SerializeField] private GameObject _buttonUi = default;
    [SerializeField] private GameState gameState = GameState.START;
    private Text _gameClear = default;
    private Image _button = default;
    private Text _restart = default;
    /// <summary>
    /// 「_fieldRootObject」はステージ解列のオブジェクト
    /// 「_textUi」,「_buttonUi」はステージクリア後に表示されるテキストとボタンUI
    /// 「_gameClear」,「_button」,「_restart」は「_textUi」,「_buttonUi」のコンポーネントを取得する変数
    /// </summary>
    //現在の手数を表示するための変数
    public int _score = default;
    //プレイ経過時間変数
    public  float _time = default;
    //現在のスコアテキスト
    public GameObject _scoreObject = default;
    //現在のタイム
    public GameObject _timeObject = default;
    /// <summary>
    /// フィールドのオブジェクトリスト
    /// 0 空欄
    /// 1 動かないブロック
    /// 2 動くブロック
    /// 3 プレイヤー
    /// 4 ゴール
    /// </summary>
    string[] _fieldObjectTagList = {
          "","StaticBlock","MoveBlock","Player","TargetPosition"
     };
   [Header("動かないオブジェクトを設定(Tagを識別する)")]
   [SerializeField] private GameObject _staticBlockTag = default;
   [Header("動くオブジェクトを設定(Tagを識別する)")]
   [SerializeField] private GameObject _moveBlockTag = default;
   [Header("プレイヤーオブジェクトを設定(Tagを識別する)")]
   [SerializeField] private GameObject _playerTag = default;
   [Header("ターゲットオブジェクトを設定(tagを識別する)")]
   [SerializeField] private GameObject _targetTag = default;
    /// <summary>
    /// フィールドデータ用の変数を定義
    /// </summary>
    private int[,] _fieldData = {
       { 0, 0, 0, 0, 0, 0},
       { 0, 0, 0, 0, 0, 0},
       { 0, 0, 0, 0, 0, 0},
       { 0, 0, 0, 0, 0, 0},
       { 0, 0, 0, 0, 0, 0},
       { 0, 0, 0, 0, 0, 0},
       };
    // 縦横の最大数
    private int _horizontalMaxCount = default;
    private int _verticalMaxCount = default;

    /// <summary>
    /// プレイヤーの位置情報
    /// </summary>
    public Vector2 PlayerPosition { get; set; }
    private int[,] _targetData = {
        { 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0 },
        };
    // ブロックがターゲットに入った数
    private int _targetClearCount = default;
    // ターゲットの最大数
    private int _targetMaxCount = default;
    /// <summary>
    /// fieldRootObjectの配下にあるオブジェクトのタグを読み取り
    /// XとY座標を基にfieldDataへ格納します(fieldDataは上書き削除します)
    /// fieldDataはfieldData[Y,X]で紐づいている
    /// フィールド初期化に使う
    /// </summary>
    /// <param name="fieldRootObject">フィールドオブジェクトのルートオブジェクトを設定</param>

    private void Awake()
    {
        SetFieldMaxSize();
        ImageToArray();
        _gameClear = _textUi.GetComponent<Text>();
        _button = _buttonUi.GetComponent<Image>();
        _restart = _buttonUi.GetComponentInChildren<Text>();
    }

    private void Update()
    {
        Text _timeText = _timeObject.GetComponent<Text>();
        Text _scoreText = _scoreObject.GetComponent<Text>();
        //経過時間を表示
        if (_targetClearCount != _targetMaxCount)
        {
            _time += Time.deltaTime;
            _timeText.text = "" + _time;
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            // 配列を出力するテスト
            print("Field------------------------------------------");
            for (int y = 0; y <= _verticalMaxCount; y++)
            {
                string outPutString = "";
                for (int x = 0; x <= _horizontalMaxCount; x++)
                {
                    outPutString += _fieldData[y, x];
                }
                print(outPutString);
            }
            print("Field------------------------------------------");
            print("プレイヤーポジション:" + PlayerPosition);
        }
        // ゲーム状態によって処理を分ける
        switch (gameState)
        {
            case GameState.START:
                SetGameState(GameState.PLAYER);
                break;
            case GameState.PLAYER:
                float horizontalInput = Input.GetAxisRaw("Horizontal");
                float verticalInput = Input.GetAxisRaw("Vertical");
                // 横入力が0より大きい場合は右に移動
                if (horizontalInput > 0 && !_inputState)
                {
                    PlayerMove(
                    Mathf.FloorToInt(PlayerPosition.x),
                    Mathf.FloorToInt(PlayerPosition.y),
                    Mathf.FloorToInt(PlayerPosition.x),
                    Mathf.FloorToInt(PlayerPosition.y + 1));
                    _inputState = true;
                }
                // 横入力が0より小さい場合は左に移動
                if (horizontalInput < 0 && !_inputState)
                {
                    PlayerMove(
                    Mathf.FloorToInt(PlayerPosition.x),
                    Mathf.FloorToInt(PlayerPosition.y),
                    Mathf.FloorToInt(PlayerPosition.x),
                    Mathf.FloorToInt(PlayerPosition.y - 1));
                    _inputState = true;
                }
                // 縦入力が0より大きい場合は上に移動
                if (verticalInput > 0 && !_inputState)
                {
                    PlayerMove(
                    Mathf.FloorToInt(PlayerPosition.x),
                    Mathf.FloorToInt(PlayerPosition.y),
                    Mathf.FloorToInt(PlayerPosition.x - 1),
                    Mathf.FloorToInt(PlayerPosition.y));
                    _inputState = true;
                }
                // 縦入力が0より小さい場合は下に移動
                if (verticalInput < 0 && !_inputState)
                {
                    PlayerMove(
                    Mathf.FloorToInt(PlayerPosition.x),
                    Mathf.FloorToInt(PlayerPosition.y),
                    Mathf.FloorToInt(PlayerPosition.x + 1),
                    Mathf.FloorToInt(PlayerPosition.y));
                    _inputState = true;
                }
                // 入力状態が解除されるまで再入力できないようにする
                if ((horizontalInput + verticalInput) == 0)
                {
                    _inputState = false;
                }
                //クリア判定
                if (GetGameClearJudgment())
                {
                    gameState = GameState.END;
                }
                break;
            case GameState.END:
                break;
        }
        //現在の手数を表示
        _scoreText.text = "" + _score;
    }

    public void ImageToArray()
    {
        // フィールドの縦と横の最大数を取得(フィールドの大きさを取得)
        foreach (Transform fieldObject in _fieldRootObject.transform)
        {
            /*
            * 縦方向に関しては座標の兼ね合い上
            * 下に行くほどyは減っていくので-をつけることで
            * yの位置を逆転させている
            */
            int col = Mathf.FloorToInt(fieldObject.position.x);
            int row = Mathf.FloorToInt(-fieldObject.position.y);
            if (_fieldObjectTagList[_staticBlock].Equals(fieldObject.tag))
            {
                _fieldData[row, col] = _staticBlock;
            }
            else if (_fieldObjectTagList[_moveBlock].Equals(fieldObject.tag))
            {
                _fieldData[row, col] = _moveBlock;
            }
            else if (_fieldObjectTagList[_player].Equals(fieldObject.tag))
            {
                _fieldData[row, col] = _player;
                PlayerPosition = new Vector2(row, col);
            }
            else if (_fieldObjectTagList[_target].Equals(fieldObject.tag))
            {
                _fieldData[row, col] = _target;
                // ターゲットの最大カウント
                _targetMaxCount++;
            }
            // フィールドデータをターゲット用のデータにコピーする
            _targetData = (int[,])_fieldData.Clone();
        }
    }
    /// <summary>
    /// フィールドのサイズを設定する
    /// フィールドの初期化に使う
    /// </summary>
    public void SetFieldMaxSize()
    {
        // フィールドの縦と横の最大数を取得(フィールドの大きさを取得)
        foreach (Transform fieldObject in _fieldRootObject.transform)
        {
            /*
            * 縦方向に関しては座標の兼ね合い上
            * 下に行くほどyは減っていくので-をつけることで
            * yの位置を逆転させている
            */
            int positionX = Mathf.FloorToInt(fieldObject.position.x);
            int positionY = Mathf.FloorToInt(-fieldObject.position.y);
            // 横の最大数を設定する
            if (_horizontalMaxCount < positionX)
            {
                _horizontalMaxCount = positionX;
            }
            // 縦の最大数を設定する
            if (_verticalMaxCount < positionY)
            {
                _verticalMaxCount = positionY;
            }
        }
        // フィールド配列の初期化
        _fieldData = new int[_verticalMaxCount + 1, _horizontalMaxCount + 1];
    }
    /// <summary>
    /// 初回起動時
    /// シーンに配置されたオブジェクトを元に配列データを生成する
    /// </summary>
   
    public GameObject GetFieldObject(int tagId, int row, int col)
    {
        foreach (Transform fieldObject in _fieldRootObject.transform)
        {
            if (tagId != -1 && fieldObject.tag
                != _fieldObjectTagList[tagId])
            {
                continue;
            }
            if (fieldObject.transform.position.x == col &&
                fieldObject.transform.position.y == -row)
            {
                return fieldObject.gameObject;
            }
        }
        return null;
    }
    public void MoveData(int preRow, int preCol, int nextRow, int nextCol)
    {
        GameObject moveObject = GetFieldObject(_fieldData[preRow, preCol], preRow, preCol);
        if (moveObject != null)
        {
            moveObject.transform.position = new Vector2(nextCol, -nextRow);
        }
        _fieldData[nextRow, nextCol] = _fieldData[preRow, preCol];
        _fieldData[preRow, preCol] = _noBlock;
    }
    public bool BlockMoveCheck(int y, int x)
    {
        // ターゲットブロックだったら
        if (_targetData[y, x] == _target)
        {
            // ターゲットクリアカウントを上げる
            _targetClearCount++;
            return true;
        }

        return _fieldData[y, x] == _noBlock;
    }
    public bool BlockMove(int preRow, int preCol, int nextRow, int nextCol)
    {
        // 境界線外エラー
        if (nextRow < 0 || nextCol < 0 ||
        nextRow > _verticalMaxCount || nextCol > _horizontalMaxCount)
        {
            return false;
        }
        bool moveFlag = BlockMoveCheck(nextRow, nextCol);
        // 移動可能かチェックする
        if (moveFlag)
        {
            // 移動が可能な場合移動する
            MoveData(preRow, preCol, nextRow, nextCol);
        }
        return moveFlag;
    }
    public bool PlayerMoveCheck(int preRow, int preCol, int nextRow, int nextCol)
    {
        /* プレイヤーの移動先が動くブロックの時
        * ブロックを移動する処理を実施する
        */
        if (_fieldData[nextRow, nextCol] == _moveBlock)
        {
            bool blockMoveFlag = BlockMove(nextRow, nextCol,
            nextRow + (nextRow - preRow),
            nextCol + (nextCol - preCol));
            return blockMoveFlag;
        }
        // プレイヤーの移動先が空の時移動する
        // プレイヤーの移動先がターゲットの時移動する
        if (_fieldData[nextRow, nextCol] == _noBlock ||
            _fieldData[nextRow, nextCol] == _target)
        {
            return true;
        }
        return false;
    }
    public void PlayerMove(int preRow, int preCol, int nextRow, int nextCol)
    {
        // 移動可能かチェックする
        if (PlayerMoveCheck(preRow, preCol, nextRow, nextCol))
        {
            // 移動が可能な場合移動する
            MoveData(preRow, preCol, nextRow, nextCol);
            // プレイヤーの位置を更新する
            // 座標情報なので最初の引数はX
            PlayerPosition = new Vector2(nextRow, nextCol);
            _score++;
        }
    }
    private enum GameState
    {
        START, PLAYER, END,
    }
    private void SetGameState(GameState gameState)
    {
        this.gameState = gameState;
    }
    private GameState GetGameState()
    {
        return this.gameState;
    }
    public bool GetGameClearJudgment()
    {
        // ターゲットクリア数とターゲットの最大数が一致したらゲームクリア
        if (_targetClearCount == _targetMaxCount)
        {
            _gameClear.enabled = true;
            _button.enabled = true;
            _restart.enabled = true;
            return true;
        }
        return false;
    }
}
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_LoginPopup : MonoBehaviour
{
    private enum Mode
    {
        Login,
        Register,
    }

    [Header("입력 칸")]
    [SerializeField] private TMP_InputField _idInput;

    [SerializeField] private TMP_InputField _passwordInput;
    [SerializeField] private TMP_InputField _passwordConfirmInput; // 회원가입 모드에서만 보인다.

    [Header("버튼")]
    [SerializeField] private Button _loginButton; // 로그인 모드: 로그인

    [SerializeField] private Button _gotoRegisterButton; // 로그인 모드: 회원가입 모드로
    [SerializeField] private Button _registerButton; // 회원가입 모드: 가입하기
    [SerializeField] private Button _gotoLoginButton; // 회원가입 모드: 로그인 모드로

    [Header("기타")]
    [SerializeField] private TextMeshProUGUI _messageText; // 안내 글자

    // 아이디 규칙: 이메일 형식 (영문, 숫자, '.', '_', '-' @ 도메인에 '.' 필요)
    //  ^                      문자열 시작
    //  [A-Za-z0-9._-]+        @ 앞부분(1글자 이상)
    //  @                      @ 하나
    //  [A-Za-z0-9_-]+         도메인 첫 구간
    //  (\.[A-Za-z0-9_-]+)+    '.' + 구간이 1번 이상 (점이 맨 뒤나 연속되면 불가)
    //  \z                     문자열 끝
    private static readonly Regex EmailRegex =
        new Regex(@"^[A-Za-z0-9._-]+@[A-Za-z0-9_-]+(\.[A-Za-z0-9_-]+)+\z");

    // 비밀번호 규칙: 대문자, 소문자, 숫자, 기호를 모두 포함한 8~30글자 (공백·한글 불가)
    //  (?=.*[A-Z])            대문자 포함
    //  (?=.*[a-z])            소문자 포함
    //  (?=.*[0-9])            숫자 포함
    //  (?=.*[^A-Za-z0-9])     기호 포함
    //  [\x21-\x7E]{8,30}      공백을 뺀 ASCII 문자만 8~30글자
    private static readonly Regex PasswordRegex =
        new Regex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[^A-Za-z0-9])[\x21-\x7E]{8,30}\z");

    private Mode _mode = Mode.Login;

    private PlayerPrefsAccountRepository _repository = new PlayerPrefsAccountRepository();

    private void Start()
    {
        Refresh();
    }

    private void Refresh()
    {
        bool isLoginMode = _mode == Mode.Login;

        _passwordConfirmInput.gameObject.SetActive(!isLoginMode);
        _loginButton.gameObject.SetActive(isLoginMode);
        _gotoRegisterButton.gameObject.SetActive(isLoginMode);

        _registerButton.gameObject.SetActive(!isLoginMode);
        _gotoLoginButton.gameObject.SetActive(!isLoginMode);
    }

    // 로그인 모드로
    public void GotoLogin()
    {
        _mode = Mode.Login;
        _messageText.text = string.Empty;
        Refresh();
    }

    // 회원가입 모드로
    public void GotoRegister()
    {
        _mode = Mode.Register;
        _messageText.text = string.Empty;
        Refresh();
    }

    public void Login()
    {
        string id = _idInput.text;
        string password = _passwordInput.text;

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password))
        {
            _messageText.text = "아이디와 비밀번호를 입력해주세요.";
            return;
        }

        if (_repository.CheckPassword(id, password))
        {
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            {
                _messageText.text = "아이디와 비밀번호를 확인해주세요.";
                return;
            }
        }
    }

    public void Register()
    {
        string id = _idInput.text;
        string password = _passwordInput.text;
        string passwordConfirm = _passwordConfirmInput.text;

        // 정규표현식(정규식)
        // 문자열에서 특정한 규칙이나 패턴을 찾기 위해 사용하는 문법
        //  - 데이터 검증(이메일 형식, 주민번호 형식, 비밀번호 형식)
        //  - 문자열 검색(ToDo라는게 있냐?, 한국어가 있냐?)
        //  - 문자열 치환(규칙에 맞는 문자열을 찾아서 다른 문자열로 변경)
        //  - 웹 크롤링 & 파싱

        // Todo: ID와 비밀번호 규칙
        // ID: 이메일
        // 비밀번호: 대문자, 소문자 숫자 기호를 포함한 8~30 글자

        // 규칙이 여기서만 쓰일까?
        // 정리 과제
        //  - 정규표현식
        //  - 명세 패턴

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password))
        {
            _messageText.text = "아이디와 비밀번호를 입력해주세요.";
            return;
        }

        // ID 규칙 : 이메일
        if (!EmailRegex.IsMatch(id))
        {
            _messageText.text = "아이디는 이메일 형식이어야 합니다.";
            return;
        }

        // 비밀번호 규칙 : 
        if (!PasswordRegex.IsMatch(password))
        {
            _messageText.text = "비밀번호는 대문자, 소문자, 숫자, 기호를 모두 포함한 8~30글자여야 합니다.";
            return;
        }

        if (password != passwordConfirm)
        {
            _messageText.text = "비밀번호 확인이 맞지 않습니다.";
            return;
        }

        // 중복 ID 검사
        if (_repository.Exists(id))
        {
            _messageText.text = "이미 존재하는 아이디 입니다.";
            return;
        }

        // 실제 회원가입 데이터 저장
        _repository.Register(id, password);

        GotoLogin();
        _messageText.text = "가입 했습니다. 로그인 해주세요.";
    }
}
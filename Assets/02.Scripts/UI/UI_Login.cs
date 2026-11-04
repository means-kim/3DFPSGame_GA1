using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Login : MonoBehaviour
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

    private Mode _mode = Mode.Login;

    private void Start()
    {
        _passwordConfirmInput.gameObject.SetActive(false);
        _registerButton.gameObject.SetActive(false);
        _gotoLoginButton.gameObject.SetActive(false);
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

        SceneManager.LoadScene("GameScene");
    }

    public void Register()
    {
    }
}
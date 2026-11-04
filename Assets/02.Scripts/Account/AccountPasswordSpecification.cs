using System.Text.RegularExpressions;

public class AccountPasswordSpecification
{
    // 비밀번호: 대문자, 소문자, 숫자, 기호를 모두 포함한 8~30글자 (공백, 한글 불가)
    //  (?=.*[A-Z])         대문자 포함
    //  (?=.*[a-z])         소문자 포함
    //  (?=.*[0-9])         숫자 포함
    //  (?=.*[^A-Za-z0-9])  기호 포함
    //  [\x21-\x7E]{8,30}   공백을 뺀 ASCII 문자만 8~30글자
    // 예) Abcd1234!, P@ssw0rd_Good
    private static readonly Regex PasswordRegex = new Regex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[^A-Za-z0-9])[\x21-\x7E]{8,30}\z");

    public string ErrorMessage => "비밀번호는 대문자, 소문자, 숫자, 기호를 모두 포함한 8~30글자여야 합니다.";

    public bool IsSatisfiedBy(string password)
    {
        return PasswordRegex.IsMatch(password);
    }
}

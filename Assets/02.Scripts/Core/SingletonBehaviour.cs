using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : class
{
    private static T _instance = null;
    public static T Instance => _instance;

    // 씬이 바뀌어도 유지할지 여부. 씬 오브젝트(UI 등)를 참조하는 매니저는 false로 둔다.
    protected virtual bool IsPersistent => false;

    protected virtual void Awake()
    {
        // T는 UnityEngine.Object 제약이 없어 != null이 참조 비교가 되므로,
        // 파괴된 인스턴스를 걸러내기 위해 UnityEngine.Object로 바꿔 비교한다.
        if (_instance as Object != null)
        {
            Destroy(this);
            return;
        }

        _instance = this as T;

        if (IsPersistent)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        // 씬 재시작 등으로 파괴될 때 정적 참조를 비워, 새 씬의 인스턴스가 등록되게 한다.
        if (ReferenceEquals(_instance, this))
        {
            _instance = null;
        }
    }
}

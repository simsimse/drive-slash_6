public interface IBossPattern
{
    float PatternDuration { get; }

    bool CanExecute();   // 지금 실행 가능한 패턴인지 확인
    void Execute();
}
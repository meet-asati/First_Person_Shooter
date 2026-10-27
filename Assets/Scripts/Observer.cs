public interface IObserver
{
    void OnSubjectChanged(ISubject subject);
}

public interface ISubject
{
    void Attach(IObserver observer);
    void Detach(IObserver observer);
    void Notify();
}
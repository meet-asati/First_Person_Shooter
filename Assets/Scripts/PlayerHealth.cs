using System.Collections.Generic;
using UnityEngine;


public class PlayerHealth : MonoBehaviour, ISubject
{
    public int health = 100;

    public AudioClip hitSFX;

    private readonly List<IObserver> observers = new List<IObserver>();

    public void Attach(IObserver observer)
    {
        if (observer != null && !observers.Contains(observer))
        {
            observers.Add(observer);
        }
    }

    public void Detach(IObserver observer)
    {
        observers.Remove(observer);
    }

    public void Notify()
    {
        foreach (IObserver observer in observers.ToArray())
        {
            observer.OnSubjectChanged(this);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Damage"))
        {
            DecreaseHealth(10);
        }
    }

    private void DecreaseHealth(int decreaseAmount)
    {
        health -= decreaseAmount;
        Notify();

        if(health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Time.timeScale = 0f;
        UIManager.Instance.EnableDeathUI();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDestroy()
    {
        observers.Clear();
    }
}

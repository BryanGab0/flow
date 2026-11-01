using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    //Vida do jogador: dano, morte por vida zerada e morte por queda
    //Contrato com armadilhas: Tag "Player" + TakeDamage(int)
    
    [Header("Vida")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invulnerabilityTime = 0.5f;

    [Header("Queda")]
    [SerializeField] private float deathHeight = -10f;

    public int CurrentHealth {get; private set;}
    public int MaxHealth => maxHealth;
    public bool IsDead {get; private set;}

    public event Action<int, int> OnHealthChanged;

    private float lastDamageTime = -999f;
    
    private void Awake()
    {
        
    }

    public void TakeDamage(int amount)
    {
        if(IsDead)
        {
            return;
        }

        if(Time.time - lastDamageTime < invulnerabilityTime)
        {
            return;
        }
        lastDamageTime = Time.time;

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if(CurrentHealth == 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (IsDead)
        {
            return;
        }
        
        IsDead = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
